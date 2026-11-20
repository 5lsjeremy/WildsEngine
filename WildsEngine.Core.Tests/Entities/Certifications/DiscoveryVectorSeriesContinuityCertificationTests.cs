using System;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class
        DiscoveryVectorSeriesContinuityCertificationTests
    {
        [Fact]
        public void
            ProductionEvaluationSeriesIsAlsoAuthoritativeVectorSeries()
        {
            var fixture =
                new H2TestFixture();

            var time =
                DateTimeOffset.UtcNow;

            var first =
                fixture.Eval(
                    time,
                    ("risk", 0.20),
                    ("trust", 0.70));

            var second =
                fixture.Eval(
                    time.AddSeconds(1),
                    ("risk", 0.50),
                    ("trust", 0.55));

            var evaluationSeries =
                fixture.Series(
                    first,
                    second);

            var vectorSeries =
                Assert.IsAssignableFrom<
                    IDiscoveryVectorSeries>(
                        evaluationSeries);

            Assert.Equal(
                evaluationSeries.Scope,
                vectorSeries.Scope);

            Assert.Equal(
                evaluationSeries.Inputs.Count,
                vectorSeries.Inputs.Count);

            for (var index = 0;
                 index < evaluationSeries.Inputs.Count;
                 index++)
            {
                Assert.Same(
                    evaluationSeries.Inputs[index],
                    vectorSeries.Inputs[index]);
            }

            Assert.Equal(
                evaluationSeries.Evaluations.Count,
                vectorSeries.Vectors.Count);

            for (var index = 0;
                 index < evaluationSeries.Evaluations.Count;
                 index++)
            {
                Assert.Same(
                    evaluationSeries
                        .Evaluations[index]
                        .Vector,
                    vectorSeries
                        .Vectors[index]);
            }

            Assert.Same(
                evaluationSeries.Current,
                second);

            Assert.Same(
                evaluationSeries.Previous,
                first);

            Assert.Same(
                second.Vector,
                vectorSeries.Current);

            Assert.Same(
                first.Vector,
                vectorSeries.Previous);
        }

        [Fact]
        public void
            SingleEvaluationPreservesVectorSeriesCurrentWithoutPrevious()
        {
            var fixture =
                new H2TestFixture();

            var evaluation =
                fixture.Eval(
                    DateTimeOffset.UtcNow,
                    ("risk", 0.20));

            var evaluationSeries =
                fixture.Series(
                    evaluation);

            var vectorSeries =
                Assert.IsAssignableFrom<
                    IDiscoveryVectorSeries>(
                        evaluationSeries);

            Assert.Single(
                vectorSeries.Vectors);

            Assert.Same(
                evaluation.Vector,
                vectorSeries.Vectors[0]);

            Assert.Same(
                evaluation.Vector,
                vectorSeries.Current);

            Assert.Null(
                vectorSeries.Previous);
        }

        [Fact]
        public void
            VectorSeriesViewPreservesEvaluationOrdering()
        {
            var fixture =
                new H2TestFixture();

            var time =
                DateTimeOffset.UtcNow;

            var first =
                fixture.Eval(
                    time,
                    ("risk", 0.10));

            var second =
                fixture.Eval(
                    time.AddSeconds(1),
                    ("risk", 0.40));

            var third =
                fixture.Eval(
                    time.AddSeconds(2),
                    ("risk", 0.90));

            var evaluationSeries =
                fixture.Series(
                    first,
                    second,
                    third);

            var vectorSeries =
                Assert.IsAssignableFrom<
                    IDiscoveryVectorSeries>(
                        evaluationSeries);

            Assert.Equal(
                3,
                vectorSeries.Vectors.Count);

            Assert.Same(
                first.Vector,
                vectorSeries.Vectors[0]);

            Assert.Same(
                second.Vector,
                vectorSeries.Vectors[1]);

            Assert.Same(
                third.Vector,
                vectorSeries.Vectors[2]);

            Assert.Same(
                third.Vector,
                vectorSeries.Current);

            Assert.Same(
                second.Vector,
                vectorSeries.Previous);
        }
    }
}