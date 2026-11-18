using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Motion;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorSeriesCertificationTests
    {
        [Fact]
        public void SeriesPreservesOrderCurrentPreviousAndExecutionIds()
        {
            var fixture = new H2TestFixture();
            var timestamp = DateTimeOffset.UtcNow;
            var first = fixture.Eval(timestamp, ("risk", 0.2));
            var second = fixture.Eval(timestamp.AddSeconds(1), ("risk", 0.5));

            var series = fixture.Series(first, second);

            Assert.Same(second, series.Current);
            Assert.Same(first, series.Previous);
            Assert.Equal(
                first.VectorizationExecutionId,
                series.Evaluations[0].VectorizationExecutionId);
        }

        [Fact]
        public void SeriesSnapshotsCallerCollections()
        {
            var fixture = new H2TestFixture();
            var evaluation = fixture.Eval(
                DateTimeOffset.UtcNow,
                ("risk", 0.2));

            var inputs =
                new List<IDiscoveryVectorInput>
                {
                    fixture.Input
                };

            var evaluations =
                new List<IDiscoveryVectorEvaluation>
                {
                    evaluation
                };

            var series =
                new DiscoveryVectorEvaluationSeriesFactory().Create(
                    "scope",
                    inputs,
                    evaluations);

            inputs.Clear();
            evaluations.Clear();

            Assert.Single(series.Inputs);
            Assert.Single(series.Evaluations);
        }

        [Fact]
        public void EqualOrDescendingTimesFail()
        {
            var fixture = new H2TestFixture();
            var timestamp = DateTimeOffset.UtcNow;
            var first = fixture.Eval(timestamp, ("risk", 0.2));
            var equal = fixture.Eval(timestamp, ("risk", 0.5));
            var descending = fixture.Eval(
                timestamp.AddSeconds(-1),
                ("risk", 0.8));

            Assert.Throws<ArgumentException>(
                () => fixture.Series(first, equal));

            Assert.Throws<ArgumentException>(
                () => fixture.Series(first, descending));
        }

        [Fact]
        public void SingleEvaluationHasCurrentWithoutPrevious()
        {
            var fixture = new H2TestFixture();
            var evaluation = fixture.Eval(
                DateTimeOffset.UtcNow,
                ("risk", 0.2));

            var series = fixture.Series(evaluation);

            Assert.Same(evaluation, series.Current);
            Assert.Null(series.Previous);
        }
    }
}
