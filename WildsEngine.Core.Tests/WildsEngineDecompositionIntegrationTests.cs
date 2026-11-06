using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class WildsEngineDecompositionIntegrationTests
{
    [Fact]
    public async Task
        PublicRequestCanOwnMultipleRequiredWorkUnits()
    {
        TestLog.Section(
            "WILDSENGINE PUBLIC MULTI-WORK DECOMPOSITION");

        TestLog.Test(
            "One Wilds-facing request owns multiple required WorkUnits.");

        var component =
            new TestComponent(
                "execute.public-decomposition");

        using var engine =
            CreateEngine(
                component);

        Assert.True(
            engine.Initialize());

        var request =
            new TestRuntimeObject(
                "consumer-request");

        var firstResult =
            new TestRuntimeObject(
                "qsl-result");

        var secondResult =
            new TestRuntimeObject(
                "instruction-result");

        var thirdResult =
            new TestRuntimeObject(
                "task-result");

        var finalResult =
            new TestRuntimeObject(
                "authoritative-final-result");

        var executionOrder =
            new List<string>();

        var operations =
            new Func<
                object?,
                CancellationToken,
                ValueTask<object?>>[]
            {
                (
                    observedRequest,
                    cancellationToken) =>
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    Assert.Same(
                        request,
                        observedRequest);

                    executionOrder.Add(
                        "QSL");

                    return new ValueTask<object?>(
                        firstResult);
                },

                (
                    observedRequest,
                    cancellationToken) =>
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    Assert.Same(
                        request,
                        observedRequest);

                    executionOrder.Add(
                        "Instruction");

                    return new ValueTask<object?>(
                        secondResult);
                },

                (
                    observedRequest,
                    cancellationToken) =>
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    Assert.Same(
                        request,
                        observedRequest);

                    executionOrder.Add(
                        "Task");

                    return new ValueTask<object?>(
                        thirdResult);
                }
            };

        TestLog.Task(
            "Submitting one request with three required operations.");

        var moment =
            await engine.ExecuteAsync(
                    request,
                    operations,
                    results =>
                    {
                        Assert.Equal(
                            3,
                            results.Count);

                        Assert.Same(
                            firstResult,
                            results[0]);

                        Assert.Same(
                            secondResult,
                            results[1]);

                        Assert.Same(
                            thirdResult,
                            results[2]);

                        return finalResult;
                    })
                .ConfigureAwait(false);

        TestLog.Task(
            "Verifying required operations executed through one request.");

        Assert.Equal(
            3,
            executionOrder.Count);

        Assert.Equal(
            "QSL",
            executionOrder[0]);

        Assert.Equal(
            "Instruction",
            executionOrder[1]);

        Assert.Equal(
            "Task",
            executionOrder[2]);

        TestLog.State(
            $"Operation order = " +
            $"{executionOrder[0]} -> " +
            $"{executionOrder[1]} -> " +
            $"{executionOrder[2]}");

        TestLog.Task(
            "Verifying one authoritative Moment owns all required work.");

        Assert.Equal(
            MomentOutcome.Completed,
            moment.Outcome);

        Assert.Same(
            finalResult,
            moment.Result);

        Assert.Null(
            moment.Exception);

        Assert.Equal(
            3,
            moment.WorkUnitIds.Count);

        Assert.NotEqual(
            Guid.Empty,
            moment.PulseId);

        Assert.NotEqual(
            Guid.Empty,
            moment.WorkUnitIds[0]);

        Assert.NotEqual(
            Guid.Empty,
            moment.WorkUnitIds[1]);

        Assert.NotEqual(
            Guid.Empty,
            moment.WorkUnitIds[2]);

        Assert.NotEqual(
            moment.WorkUnitIds[0],
            moment.WorkUnitIds[1]);

        Assert.NotEqual(
            moment.WorkUnitIds[0],
            moment.WorkUnitIds[2]);

        Assert.NotEqual(
            moment.WorkUnitIds[1],
            moment.WorkUnitIds[2]);

        TestLog.State(
            $"Pulse = {moment.PulseId}");

        TestLog.State(
            $"Required WorkUnits = {moment.WorkUnitIds.Count}");

        TestLog.Task(
            "Verifying subordinate results did not become the Pulse result.");

        Assert.False(
            ReferenceEquals(
                firstResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                secondResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                thirdResult,
                moment.Result));

        Assert.Same(
            finalResult,
            moment.Result);

        TestLog.Pass(
            "Result selector supplied the authoritative Pulse result.");

        TestLog.Task(
            "Verifying the Moment entered Wilds temporal history exactly once.");

        Assert.Equal(
            1,
            engine.Timeline.Count);

        Assert.Single(
            engine.Timeline.Moments);

        Assert.Same(
            moment,
            engine.Timeline.Moments[0]);

        Assert.Same(
            finalResult,
            engine.Timeline
                .Moments[0]
                .Result);

        Assert.Equal(
            1,
            engine.Epoch.Count);

        Assert.Single(
            engine.Epoch.Timelines);

        Assert.Same(
            engine.Timeline,
            engine.Epoch.Timelines[0]);

        Assert.Single(
            engine.Epoch
                .Timelines[0]
                .Moments);

        Assert.Same(
            moment,
            engine.Epoch
                .Timelines[0]
                .Moments[0]);

        Assert.Same(
            finalResult,
            engine.Epoch
                .Timelines[0]
                .Moments[0]
                .Result);

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Timeline Moments = {engine.Timeline.Count}");

        TestLog.State(
            $"Epoch Timelines = {engine.Epoch.Count}");

        TestLog.Pass(
            "One Wilds-facing request owned three required WorkUnits.");

        TestLog.Pass(
            "Wilds produced one authoritative completed Moment.");

        TestLog.Pass(
            "Subordinate results did not leak into the final result.");

        TestLog.Pass(
            "The completed Moment survived Timeline and Epoch history.");

        TestLog.Pass(
            "Public multi-work decomposition certification completed successfully.");
    }

    private static Core.WildsEngine CreateEngine(
        IComponent component)
    {
        return new Core.WildsEngine(
            component,
            new WildsEngineOptions
            {
                Layer =
                    "certification.execute.decomposition",

                RetainedInstructionsProcessors =
                    1,

                ConcurrentInstructionsProcessors =
                    1,

                RetainedTaskProcessors =
                    1,

                ConcurrentTaskProcessors =
                    1
            });
    }

    private sealed class TestRuntimeObject
    {
        public TestRuntimeObject(
            string name)
        {
            Name =
                name;
        }

        public string Name { get; }
    }
}