using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class TemporalConcurrencyOrderingCertificationTests
{
    [Fact]
    public async Task ConcurrentTimelineCommitsPreserveSuccessfulSerializedOrder()
    {
        TestLog.Section(
            "TEMPORAL CONCURRENCY - TIMELINE COMMIT ORDER");

        TestLog.Test(
            "Concurrent Moment commits produce one coherent serialized Timeline order.");

        const int count =
            64;

        var timeline =
            new Timeline();

        var moments =
            new Moment[count];

        for (var index = 0;
             index < count;
             index++)
        {
            moments[index] =
                new Moment(
                    Guid.NewGuid(),
                    index,
                    new[]
                    {
                        Guid.NewGuid()
                    });
        }

        var readyCount =
            0;

        var allReady =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var release =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks =
            new Task<bool>[count];

        for (var index = 0;
             index < count;
             index++)
        {
            var captured =
                moments[index];

            tasks[index] =
                Task.Run(
                    async () =>
                    {
                        if (Interlocked.Increment(
                                ref readyCount) == count)
                        {
                            allReady.TrySetResult(
                                true);
                        }

                        await release.Task
                            .ConfigureAwait(false);

                        return timeline.AddMoment(
                            captured);
                    });
        }

        TestLog.Task(
            "Waiting until every concurrent writer is ready.");

        await allReady.Task
            .WaitAsync(
                TimeSpan.FromSeconds(
                    10));

        TestLog.Task(
            "Releasing all Timeline writers concurrently.");

        release.TrySetResult(
            true);

        var committed =
            await Task.WhenAll(
                tasks);

        for (var index = 0;
             index < committed.Length;
             index++)
        {
            Assert.True(
                committed[index]);
        }

        var history =
            timeline.Moments;

        Assert.Equal(
            count,
            timeline.Count);

        Assert.Equal(
            count,
            history.Count);

        var expectedIds =
            new HashSet<Guid>();

        for (var index = 0;
             index < moments.Length;
             index++)
        {
            Assert.True(
                expectedIds.Add(
                    moments[index].Id));
        }

        var committedIds =
            new HashSet<Guid>();

        for (var index = 0;
             index < history.Count;
             index++)
        {
            Assert.True(
                committedIds.Add(
                    history[index].Id));

            Assert.Contains(
                history[index].Id,
                expectedIds);
        }

        Assert.Equal(
            count,
            committedIds.Count);

        /*
         * We intentionally do not assert that Timeline order
         * equals creation, submission, task-start, or task-
         * completion order.
         *
         * The Timeline contract is successful serialized commit
         * order. The observable Moments snapshot is the
         * authoritative historical sequence.
         */

        TestLog.State(
            $"Concurrent commits = {count}");

        TestLog.State(
            $"Timeline Moments = {history.Count}");

        TestLog.State(
            $"Unique Moment identities = {committedIds.Count}");

        TestLog.Pass(
            "Every concurrent Moment was committed exactly once.");

        TestLog.Pass(
            "Timeline exposed one coherent serialized history.");

        TestLog.Pass(
            "No unsupported submission or completion ordering was assumed.");
    }

    [Fact]
    public async Task ConcurrentExecuteAsyncHistoryUsesTimelineCommitOrder()
    {
        TestLog.Section(
            "TEMPORAL CONCURRENCY - ENGINE TIMELINE ORDER");

        TestLog.Test(
            "Concurrent Wilds executions expose their terminal Moments in committed Timeline order.");

        const int count =
            32;

        using var engine =
            CreateEngine(
                "timeline-order-certification");

        Assert.True(
            engine.Initialize());

        var requests =
            new object[count];

        for (var index = 0;
             index < count;
             index++)
        {
            requests[index] =
                new object();
        }

        var readyCount =
            0;

        var allReady =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var release =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var executions =
            new Task<IMoment>[count];

        for (var index = 0;
             index < count;
             index++)
        {
            var request =
                requests[index];

            executions[index] =
                Task.Run(
                    async () =>
                    {
                        return await engine.ExecuteAsync(
                            request,
                            async (
                                value,
                                cancellationToken) =>
                            {
                                if (Interlocked.Increment(
                                        ref readyCount) == count)
                                {
                                    allReady.TrySetResult(
                                        true);
                                }

                                await release.Task
                                    .WaitAsync(
                                        cancellationToken)
                                    .ConfigureAwait(false);

                                return value;
                            });
                    });
        }

        TestLog.Task(
            "Waiting until every concurrent Wilds operation is active.");

        await allReady.Task
            .WaitAsync(
                TimeSpan.FromSeconds(
                    10));

        TestLog.Task(
            "Releasing concurrent Wilds operations.");

        release.TrySetResult(
            true);

        var returned =
            await Task.WhenAll(
                executions);

        var history =
            engine.Timeline.Moments;

        Assert.Equal(
            count,
            returned.Length);

        Assert.Equal(
            count,
            engine.Timeline.Count);

        Assert.Equal(
            count,
            history.Count);

        var returnedById =
            new Dictionary<Guid, IMoment>();

        for (var index = 0;
             index < returned.Length;
             index++)
        {
            Assert.True(
                returnedById.TryAdd(
                    returned[index].Id,
                    returned[index]));
        }

        var historyIds =
            new HashSet<Guid>();

        for (var index = 0;
             index < history.Count;
             index++)
        {
            var committed =
                history[index];

            Assert.True(
                historyIds.Add(
                    committed.Id));

            Assert.True(
                returnedById.TryGetValue(
                    committed.Id,
                    out var returnedMoment));

            Assert.Same(
                returnedMoment,
                committed);
        }

        Assert.Equal(
            count,
            returnedById.Count);

        Assert.Equal(
            count,
            historyIds.Count);

        Assert.Single(
            engine.Epoch.Timelines);

        Assert.Same(
            engine.Timeline,
            engine.Epoch.Timelines[0]);

        var epochHistory =
            engine.Epoch.Timelines[0]
                .Moments;

        Assert.Equal(
            history.Count,
            epochHistory.Count);

        for (var index = 0;
             index < history.Count;
             index++)
        {
            Assert.Same(
                history[index],
                epochHistory[index]);
        }

        /*
         * Timeline.Moments is the authoritative observation of
         * serialized commit order.
         *
         * This test intentionally does not equate that order with
         * request submission, Pulse creation, operation completion,
         * or pre-commit finalization.
         */

        TestLog.State(
            $"Concurrent executions = {count}");

        TestLog.State(
            $"Returned Moments = {returned.Length}");

        TestLog.State(
            $"Committed Timeline Moments = {history.Count}");

        TestLog.State(
            $"Epoch Timeline Moments = {epochHistory.Count}");

        TestLog.Pass(
            "Every returned Moment appeared exactly once in committed history.");

        TestLog.Pass(
            "Timeline and Epoch exposed the same committed Moment sequence.");

        TestLog.Pass(
            "Concurrent Wilds history remained coherent under the successful-commit ordering contract.");

        TestLog.Pass(
            "The certification did not impose an unsupported concurrent execution order.");
    }

    private static WildsEngine CreateEngine(
        string layer)
    {
        return new WildsEngine(
            new TestComponent(
                layer),
            new WildsEngineOptions
            {
                Layer =
                    layer,

                RetainedInstructionsProcessors =
                    8,

                ConcurrentInstructionsProcessors =
                    64,

                RetainedTaskProcessors =
                    16,

                ConcurrentTaskProcessors =
                    128
            });
    }
}
