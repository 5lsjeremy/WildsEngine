using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies the Wilds unit and temporal foundation:
    ///
    /// BaseUnit
    ///     -> Pulse
    ///         -> WorkUnit(s)
    ///             -> Moment
    ///                 -> Timeline
    ///                     -> Epoch
    ///
    /// A Pulse represents one complete request.
    /// WorkUnits represent internal processing required
    /// to resolve that Pulse.
    /// A Moment records the completed outcome.
    /// Timelines preserve ordered Moments.
    /// Epochs preserve collections of Timelines.
    /// </summary>
    public sealed class UnitCertificationTests
    {
        [Fact]
        public void PulseReceivesStableNonEmptyIdentity()
        {
            var request =
                new TestRuntimeObject(
                    "request");

            var pulse =
                new Pulse(
                    request);

            Assert.NotEqual(
                Guid.Empty,
                pulse.Id);

            Assert.Same(
                request,
                pulse.Request);

            Assert.Equal(
                PulseStatus.Created,
                pulse.Status);

            Assert.False(
                pulse.IsTerminal);
        }

        [Fact]
        public void PulseEnforcesLifecycleOrder()
        {
            var pulse =
                new Pulse(
                    new TestRuntimeObject(
                        "request"));

            Assert.False(
                pulse.TryMarkPending());

            Assert.False(
                pulse.TryMarkReady());

            Assert.False(
                pulse.TryBeginProcessing());

            Assert.False(
                pulse.TryComplete());

            Assert.True(
                pulse.TryBeginDecomposition());

            Assert.Equal(
                PulseStatus.Decomposing,
                pulse.Status);

            Assert.True(
                pulse.TryMarkPending());

            Assert.Equal(
                PulseStatus.Pending,
                pulse.Status);

            Assert.True(
                pulse.TryMarkReady());

            Assert.Equal(
                PulseStatus.Ready,
                pulse.Status);

            Assert.True(
                pulse.TryBeginProcessing());

            Assert.Equal(
                PulseStatus.Processing,
                pulse.Status);

            Assert.True(
                pulse.TryComplete());

            Assert.Equal(
                PulseStatus.Completed,
                pulse.Status);

            Assert.True(
                pulse.IsTerminal);

            Assert.False(
                pulse.TryComplete());

            Assert.False(
                pulse.TryFail());

            Assert.False(
                pulse.TryCancel());
        }

        [Fact]
        public void MultipleWorkUnitsBelongToOnePulse()
        {
            var pulse =
                new Pulse(
                    new TestRuntimeObject(
                        "request"));

            var first =
                new WorkUnit(
                    pulse.Id,
                    new TestRuntimeObject(
                        "first"));

            var second =
                new WorkUnit(
                    pulse.Id,
                    new TestRuntimeObject(
                        "second"));

            Assert.NotEqual(
                Guid.Empty,
                first.Id);

            Assert.NotEqual(
                Guid.Empty,
                second.Id);

            Assert.NotEqual(
                first.Id,
                second.Id);

            Assert.Equal(
                pulse.Id,
                first.PulseId);

            Assert.Equal(
                pulse.Id,
                second.PulseId);
        }

        [Fact]
        public void WorkUnitRejectsEmptyPulseIdentity()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ =
                        new WorkUnit(
                            Guid.Empty,
                            new object());
                });
        }

        [Fact]
        public void WorkUnitEnforcesLifecycleOrder()
        {
            var pulse =
                new Pulse(
                    "request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "work");

            Assert.Equal(
                WorkUnitStatus.Created,
                work.Status);

            Assert.False(
                work.TryBeginProcessing());

            Assert.False(
                work.TryComplete(
                    "too-early"));

            Assert.True(
                work.TryMarkPending());

            Assert.Equal(
                WorkUnitStatus.Pending,
                work.Status);

            Assert.True(
                work.TryBeginProcessing());

            Assert.Equal(
                WorkUnitStatus.Processing,
                work.Status);

            var result =
                new TestRuntimeObject(
                    "result");

            Assert.True(
                work.TryComplete(
                    result));

            Assert.Equal(
                WorkUnitStatus.Completed,
                work.Status);

            Assert.True(
                work.IsTerminal);

            Assert.Same(
                result,
                work.Result);

            Assert.Null(
                work.Exception);

            Assert.False(
                work.TryComplete(
                    new object()));

            Assert.False(
                work.TryCancel());

            Assert.False(
                work.TryFail(
                    new InvalidOperationException()));
        }

        [Fact]
        public void WorkUnitPreservesFailure()
        {
            var pulse =
                new Pulse(
                    "request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "work");

            var exception =
                new InvalidOperationException(
                    "Expected failure.");

            Assert.True(
                work.TryFail(
                    exception));

            Assert.Equal(
                WorkUnitStatus.Failed,
                work.Status);

            Assert.True(
                work.IsTerminal);

            Assert.Same(
                exception,
                work.Exception);

            Assert.Null(
                work.Result);

            Assert.False(
                work.TryCancel());
        }

        [Fact]
        public void WorkUnitSupportsCancellation()
        {
            var pulse =
                new Pulse(
                    "request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "work");

            Assert.True(
                work.TryMarkPending());

            Assert.True(
                work.TryCancel());

            Assert.Equal(
                WorkUnitStatus.Cancelled,
                work.Status);

            Assert.True(
                work.IsTerminal);

            Assert.False(
                work.TryBeginProcessing());
        }

        [Fact]
        public void MomentRecordsPulseWorkUnitsAndFinalResult()
        {
            var pulse =
                new Pulse(
                    "request");

            var first =
                new WorkUnit(
                    pulse.Id,
                    "first");

            var second =
                new WorkUnit(
                    pulse.Id,
                    "second");

            var result =
                new TestRuntimeObject(
                    "final");

            var moment =
                new Moment(
                    pulse.Id,
                    result,
                    new[]
                    {
                        first.Id,
                        second.Id
                    });

            Assert.NotEqual(
                Guid.Empty,
                moment.Id);

            Assert.Equal(
                pulse.Id,
                moment.PulseId);

            Assert.Same(
                result,
                moment.Result);

            Assert.Equal(
                2,
                moment.WorkUnitIds.Count);

            Assert.Contains(
                first.Id,
                moment.WorkUnitIds);

            Assert.Contains(
                second.Id,
                moment.WorkUnitIds);
        }

        [Fact]
        public void MomentRemovesDuplicateWorkUnitIdentities()
        {
            var pulse =
                new Pulse(
                    "request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "work");

            var moment =
                new Moment(
                    pulse.Id,
                    "result",
                    new[]
                    {
                        work.Id,
                        work.Id,
                        Guid.Empty
                    });

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Equal(
                work.Id,
                moment.WorkUnitIds[0]);
        }

        [Fact]
        public void TimelinePreservesMomentOrderAndRejectsDuplicates()
        {
            var timeline =
                new Timeline();

            var firstPulse =
                new Pulse(
                    "first");

            var secondPulse =
                new Pulse(
                    "second");

            var firstMoment =
                new Moment(
                    firstPulse.Id,
                    "first-result");

            var secondMoment =
                new Moment(
                    secondPulse.Id,
                    "second-result");

            Assert.True(
                timeline.AddMoment(
                    firstMoment));

            Assert.True(
                timeline.AddMoment(
                    secondMoment));

            Assert.False(
                timeline.AddMoment(
                    firstMoment));

            Assert.Equal(
                2,
                timeline.Count);

            Assert.Same(
                firstMoment,
                timeline.Moments[0]);

            Assert.Same(
                secondMoment,
                timeline.Moments[1]);
        }

        [Fact]
        public void EpochPreservesTimelinesAndRejectsDuplicates()
        {
            var epoch =
                new Epoch();

            var first =
                new Timeline();

            var second =
                new Timeline();

            Assert.True(
                epoch.AddTimeline(
                    first));

            Assert.True(
                epoch.AddTimeline(
                    second));

            Assert.False(
                epoch.AddTimeline(
                    first));

            Assert.Equal(
                2,
                epoch.Count);

            Assert.Same(
                first,
                epoch.Timelines[0]);

            Assert.Same(
                second,
                epoch.Timelines[1]);
        }

        [Fact]
        
        public void ArbitraryRuntimeObjectSurvivesCompleteUnitRoute()
        {
            TestLog.Section(
                "Wilds Temporal Unit Certification");

            var runtimeObject =
                new TestRuntimeObject(
                    "consumer-runtime-object");

            var pulse =
                new Pulse(
                    runtimeObject);

            TestLog.Test(
                "Complete Pulse -> WorkUnit -> Moment -> Timeline -> Epoch route");

            TestLog.State(
                $"Pulse created | Id={pulse.Id} | Status={pulse.Status}");

            Assert.Same(
                runtimeObject,
                pulse.Request);

            Assert.True(
                pulse.TryBeginDecomposition());

            TestLog.State(
                $"Pulse decomposing | Status={pulse.Status}");

            var work =
                new WorkUnit(
                    pulse.Id,
                    pulse.Request);

            TestLog.Task(
                $"WorkUnit created | Id={work.Id} | PulseId={work.PulseId}");

            Assert.Same(
                runtimeObject,
                work.Payload);

            TestLog.State(
                "Request reference preserved in WorkUnit payload");

            Assert.True(
                work.TryMarkPending());

            Assert.True(
                pulse.TryMarkPending());

            TestLog.State(
                $"Pulse pending | Status={pulse.Status}");

            Assert.True(
                pulse.TryMarkReady());

            TestLog.State(
                $"Pulse ready | Status={pulse.Status}");

            Assert.True(
                pulse.TryBeginProcessing());

            Assert.True(
                work.TryBeginProcessing());

            TestLog.Task(
                $"WorkUnit processing | Status={work.Status}");

            Assert.True(
                work.TryComplete(
                    runtimeObject));

            Assert.Same(
                runtimeObject,
                work.Result);

            TestLog.Task(
                $"WorkUnit completed | Status={work.Status}");

            TestLog.State(
                "Runtime object reference preserved through WorkUnit result");

            Assert.True(
                pulse.TryComplete());

            TestLog.State(
                $"Pulse completed | Status={pulse.Status}");

            var moment =
                new Moment(
                    pulse.Id,
                    work.Result,
                    new[]
                    {
                        work.Id
                    });

            Assert.Equal(
                pulse.Id,
                moment.PulseId);

            Assert.Same(
                runtimeObject,
                moment.Result);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Equal(
                work.Id,
                moment.WorkUnitIds[0]);

            TestLog.State(
                $"Moment recorded | Id={moment.Id} | PulseId={moment.PulseId}");

            var timeline =
                new Timeline();

            Assert.True(
                timeline.AddMoment(
                    moment));

            TestLog.State(
                $"Timeline recorded Moment | Moments={timeline.Count}");

            var epoch =
                new Epoch();

            Assert.True(
                epoch.AddTimeline(
                    timeline));

            TestLog.State(
                $"Epoch recorded Timeline | Timelines={epoch.Count}");

            Assert.Same(
                runtimeObject,
                epoch.Timelines[0]
                    .Moments[0]
                    .Result);

            TestLog.Pass(
                "Exact runtime object survived " +
                "Pulse -> WorkUnit -> Moment -> Timeline -> Epoch");
        }

        private sealed class TestRuntimeObject
        {
            public TestRuntimeObject(
                string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}