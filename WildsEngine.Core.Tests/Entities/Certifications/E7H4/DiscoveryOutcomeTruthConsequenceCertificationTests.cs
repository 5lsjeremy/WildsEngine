using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Truths;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Consequences;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Narratives;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Discoveries.Truths;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class
        DiscoveryOutcomeTruthConsequenceCertificationTests
    {
        [Fact]
        public void
            H4C_RetainsCrossGroupRejectedDecisionAndDesignerDefinition()
        {
            var fixture =
                Fixture.Create();

            var relationship =
                new DiscoveryNarrativeRelationshipDefinition(
                    Guid.NewGuid(),
                    "competing",
                    fixture.FirstKey,
                    fixture.SecondKey,
                    "CompetingOutcome");

            var profile =
                new DiscoveryNarrativeProfile(
                    Guid.NewGuid(),
                    "gobbo-narratives",
                    "1.0",
                    fixture.Scope,
                    new RelationshipKeyNarrativeCompositionPolicy(
                        "ReinforcingOutcome"),
                    new RecognitionCentroidDistanceNarrativeMetric(),
                    new List<IDiscoveryNarrativeRelationshipDefinition>
                    {
                        relationship
                    });

            var registration =
                fixture.Discoveries.Narratives.Register(
                    profile);

            var composition =
                fixture.Discoveries.Narratives.Compose(
                    fixture.Population,
                    registration.Id);

            Assert.Equal(
                2,
                composition.Groups.Count);

            Assert.Single(
                composition.Decisions);

            Assert.False(
                composition.Decisions[0].CanCompose);

            Assert.Same(
                relationship,
                composition.Decisions[0].Relationship);

            var competing =
                fixture.Discoveries.Outcomes.Relate(
                    composition);

            Assert.Single(
                competing.GroupRelationships);

            var relation =
                competing.GroupRelationships[0];

            Assert.Single(
                relation.Decisions);

            Assert.Same(
                composition.Decisions[0],
                relation.Decisions[0]);

            Assert.Single(
                relation.Definitions);

            Assert.Same(
                relationship,
                relation.Definitions[0]);
        }

        [Fact]
        public void
            H5_RefusedCongealmentProducesNoTruthAndNeverCallsCongeal()
        {
            var fixture =
                Fixture.Create();

            var competing =
                fixture.Competing();

            var evaluationSeries =
                fixture.Series();

            var series =
                Assert.IsAssignableFrom<IDiscoveryVectorSeries>(
                    evaluationSeries);

            var clusters =
                fixture.Clusters(
                    series);

            var policy =
                new TestTruthPolicy(
                    false);

            var profile =
                new DiscoveryTruthProfile(
                    Guid.NewGuid(),
                    "gobbo-truth",
                    "1.0",
                    fixture.Scope,
                    policy);

            var registration =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Register(
                        profile);

            var result =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        competing,
                        registration.Id,
                        series,
                        clusters);

            Assert.False(
                result.Congealed);

            Assert.Empty(
                result.Truths);

            Assert.False(
                policy.CongealCalled);

            Assert.Same(
                competing,
                policy.CanCongealNarratives);

            Assert.Same(
                clusters,
                policy.CanCongealClusters);

            Assert.Same(
                series,
                policy.CanCongealSeries);

            Assert.Null(
                policy.CongealNarratives);

            Assert.Null(
                policy.CongealClusters);

            Assert.Null(
                policy.CongealSeries);

            Assert.Same(
                evaluationSeries,
                result.Series);

            Assert.Same(
                series,
                result.Series);

            Assert.Same(
                competing,
                result.Narratives);

            Assert.Equal(
                policy.Id,
                result.Registration.CongealmentPolicyId);
        }

        [Fact]
        public void
            H5_ApprovedCongealmentRetainsExactProductionSeriesClustersAndPolicyIdentity()
        {
            var fixture =
                Fixture.Create();

            var competing =
                fixture.Competing();

            var evaluationSeries =
                fixture.Series();

            var series =
                Assert.IsAssignableFrom<IDiscoveryVectorSeries>(
                    evaluationSeries);

            var clusters =
                fixture.Clusters(
                    series);

            var policy =
                new TestTruthPolicy(
                    true);

            var profile =
                new DiscoveryTruthProfile(
                    Guid.NewGuid(),
                    "gobbo-truth",
                    "1.0",
                    fixture.Scope,
                    policy);

            var registration =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Register(
                        profile);

            var result =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        competing,
                        registration.Id,
                        series,
                        clusters);

            Assert.True(
                result.Congealed);

            Assert.Single(
                result.Truths);

            Assert.True(
                policy.CongealCalled);

            Assert.Same(
                competing,
                policy.CanCongealNarratives);

            Assert.Same(
                competing,
                policy.CongealNarratives);

            Assert.Same(
                clusters,
                policy.CanCongealClusters);

            Assert.Same(
                clusters,
                policy.CongealClusters);

            Assert.Same(
                series,
                policy.CanCongealSeries);

            Assert.Same(
                series,
                policy.CongealSeries);

            Assert.Same(
                evaluationSeries,
                result.Series);

            var truth =
                result.Truths[0];

            Assert.Same(
                evaluationSeries,
                truth.Series);

            Assert.Same(
                series,
                truth.Series);

            Assert.Equal(
                clusters.Count,
                truth.Clusters.Count);

            for (var index = 0;
                 index < clusters.Count;
                 index++)
            {
                Assert.Same(
                    clusters[index],
                    truth.Clusters[index]);
            }

            Assert.Equal(
                policy.Id,
                result.Registration.CongealmentPolicyId);
        }

        [Fact]
        public void
            D4_CompetingNarrativesInformConsumerTruthDecision()
        {
            var fixture =
                Fixture.Create();

            // Both calls begin with the same recognized population,
            // scope, geometry, series, and clusters.
            //
            // Each call produces a distinct competing-narrative
            // result, allowing the consumer policy to distinguish
            // the supplied narrative contexts.
            var acceptedNarratives =
                fixture.Competing();

            var refusedNarratives =
                fixture.Competing();

            Assert.NotSame(
                acceptedNarratives,
                refusedNarratives);

            var evaluationSeries =
                fixture.Series();

            var series =
                Assert.IsAssignableFrom<IDiscoveryVectorSeries>(
                    evaluationSeries);

            var clusters =
                fixture.Clusters(
                    series);

            // This consumer accepts one exact narrative context and
            // refuses another.
            //
            // Discoveries delivers the context. It does not decide
            // what the narratives mean or whether Truth follows.
            var policy =
                new NarrativeSensitiveTruthPolicy(
                    acceptedNarratives);

            var profile =
                new DiscoveryTruthProfile(
                    Guid.NewGuid(),
                    "gobbo-narrative-sensitive-truth",
                    "1.0",
                    fixture.Scope,
                    policy);

            var registration =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Register(
                        profile);

            var acceptedResult =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        acceptedNarratives,
                        registration.Id,
                        series,
                        clusters);

            Assert.True(
                acceptedResult.Congealed);

            Assert.Single(
                acceptedResult.Truths);

            Assert.True(
                policy.CongealCalled);

            Assert.Same(
                acceptedNarratives,
                policy.CanCongealNarratives);

            Assert.Same(
                acceptedNarratives,
                policy.CongealNarratives);

            Assert.Same(
                clusters,
                policy.CanCongealClusters);

            Assert.Same(
                clusters,
                policy.CongealClusters);

            Assert.Same(
                series,
                policy.CanCongealSeries);

            Assert.Same(
                series,
                policy.CongealSeries);

            for (var index = 0;
                 index < clusters.Count;
                 index++)
            {
                Assert.Same(
                    clusters[index],
                    policy.CanCongealClusters![index]);

                Assert.Same(
                    clusters[index],
                    policy.CongealClusters![index]);
            }

            Assert.Same(
                acceptedNarratives,
                acceptedResult.Narratives);

            Assert.Same(
                series,
                acceptedResult.Series);

            policy.ResetCallEvidence();

            var refusedResult =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        refusedNarratives,
                        registration.Id,
                        series,
                        clusters);

            // The exact clusters and exact vector series are
            // unchanged. Only the narrative context differs.
            //
            // The consumer is allowed to refuse Truth for that
            // reason.
            Assert.False(
                refusedResult.Congealed);

            Assert.Empty(
                refusedResult.Truths);

            Assert.False(
                policy.CongealCalled);

            Assert.Same(
                refusedNarratives,
                policy.CanCongealNarratives);

            Assert.Same(
                clusters,
                policy.CanCongealClusters);

            Assert.Same(
                series,
                policy.CanCongealSeries);

            for (var index = 0;
                 index < clusters.Count;
                 index++)
            {
                Assert.Same(
                    clusters[index],
                    policy.CanCongealClusters![index]);
            }

            // CanCongeal refused the context, so the policy must not
            // be asked to produce Truth.
            Assert.Null(
                policy.CongealNarratives);

            Assert.Null(
                policy.CongealClusters);

            Assert.Null(
                policy.CongealSeries);

            Assert.Same(
                refusedNarratives,
                refusedResult.Narratives);

            Assert.Same(
                series,
                refusedResult.Series);
        }

        [Fact]
        public void
            D3_RefusedTruthCannotInvokeConsequenceProjection()
        {
            var fixture =
                Fixture.Create();

            var competing =
                fixture.Competing();

            var evaluationSeries =
                fixture.Series();

            var series =
                Assert.IsAssignableFrom<IDiscoveryVectorSeries>(
                    evaluationSeries);

            var clusters =
                fixture.Clusters(
                    series);

            var truthPolicy =
                new TestTruthPolicy(
                    false);

            var truthProfile =
                new DiscoveryTruthProfile(
                    Guid.NewGuid(),
                    "gobbo-refused-truth",
                    "1.0",
                    fixture.Scope,
                    truthPolicy);

            var truthRegistration =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Register(
                        truthProfile);

            var truthResult =
                fixture.Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        competing,
                        truthRegistration.Id,
                        series,
                        clusters);

            Assert.False(
                truthResult.Congealed);

            Assert.Empty(
                truthResult.Truths);

            Assert.False(
                truthPolicy.CongealCalled);

            Assert.Same(
                competing,
                truthPolicy.CanCongealNarratives);

            var consequencePolicy =
                new TestConsequencePolicy(
                    project: true);

            var consequenceProfile =
                new DiscoveryConsequenceProfile(
                    Guid.NewGuid(),
                    "gobbo-refused-consequence",
                    "1.0",
                    fixture.Scope,
                    consequencePolicy);

            var consequenceRegistration =
                fixture.Discoveries
                    .Outcomes
                    .Consequences
                    .Register(
                        consequenceProfile);

            Assert.Throws<InvalidOperationException>(
                () =>
                    fixture.Discoveries
                        .Outcomes
                        .Consequences
                        .Project(
                            truthResult,
                            consequenceRegistration.Id));

            Assert.False(
                consequencePolicy.ProjectCalled);
        }

        [Fact]
        public void H6_EmptyConsumerProjectionIsValid()
        {
            var fixture =
                Fixture.Create();

            var truthResult =
                fixture.CongealedTruth();

            var policy =
                new TestConsequencePolicy(
                    project: false);

            var profile =
                new DiscoveryConsequenceProfile(
                    Guid.NewGuid(),
                    "gobbo-consequence",
                    "1.0",
                    fixture.Scope,
                    policy);

            var registration =
                fixture.Discoveries
                    .Outcomes
                    .Consequences
                    .Register(
                        profile);

            var result =
                fixture.Discoveries
                    .Outcomes
                    .Consequences
                    .Project(
                        truthResult,
                        registration.Id);

            Assert.True(
                policy.ProjectCalled);

            Assert.Empty(
                result.Consequences);

            Assert.Same(
                truthResult,
                result.TruthResult);

            Assert.Equal(
                policy.Id,
                result.Registration.ProjectionPolicyId);
        }

        [Fact]
        public void H6_ProjectedConsequenceRetainsExactTruth()
        {
            var fixture =
                Fixture.Create();

            var truthResult =
                fixture.CongealedTruth();

            var policy =
                new TestConsequencePolicy(
                    project: true);

            var profile =
                new DiscoveryConsequenceProfile(
                    Guid.NewGuid(),
                    "gobbo-consequence",
                    "1.0",
                    fixture.Scope,
                    policy);

            var registration =
                fixture.Discoveries
                    .Outcomes
                    .Consequences
                    .Register(
                        profile);

            var result =
                fixture.Discoveries
                    .Outcomes
                    .Consequences
                    .Project(
                        truthResult,
                        registration.Id);

            Assert.True(
                policy.ProjectCalled);

            Assert.Single(
                result.Consequences);

            var consequence =
                result.Consequences[0];

            Assert.Equal(
                "GobboMayTurnToStone",
                consequence.Name);

            Assert.Single(
                consequence.Truths);

            Assert.Same(
                truthResult.Truths[0],
                consequence.Truths[0]);

            Assert.Equal(
                policy.Id,
                result.Registration.ProjectionPolicyId);
        }

        private sealed class Fixture
        {
            private readonly H2TestFixture
                _firstFixture;

            private Fixture(
                WildsDiscoveries discoveries,
                IDiscoveryRecognizedGeometryPopulation population,
                string scope,
                string firstKey,
                string secondKey,
                IDiscoveryClusterRecognitionResult first,
                IDiscoveryClusterRecognitionResult second,
                H2TestFixture firstFixture)
            {
                Discoveries =
                    discoveries;

                Population =
                    population;

                Scope =
                    scope;

                FirstKey =
                    firstKey;

                SecondKey =
                    secondKey;

                First =
                    first;

                Second =
                    second;

                _firstFixture =
                    firstFixture;
            }

            public WildsDiscoveries Discoveries { get; }

            public IDiscoveryRecognizedGeometryPopulation
                Population { get; }

            public string Scope { get; }

            public string FirstKey { get; }

            public string SecondKey { get; }

            public IDiscoveryClusterRecognitionResult
                First { get; }

            public IDiscoveryClusterRecognitionResult
                Second { get; }

            public static Fixture Create()
            {
                const string scope =
                    "gobbo-temple-run";

                const string firstKey =
                    "EscapeStillPossible";

                const string secondKey =
                    "FailureBecomingLikely";

                var discoveries =
                    new WildsDiscoveries();

                var profileId =
                    Guid.NewGuid();

                var maps =
                    new List<IDiscoveryClusterMapping>
                    {
                        Map(
                            profileId,
                            "escape",
                            firstKey,
                            0),

                        Map(
                            profileId,
                            "failure",
                            secondKey,
                            10)
                    };

                var geometryRegistration =
                    discoveries.Geometry.Register(
                        new DiscoveryGeometryProfile(
                            profileId,
                            "gobbo-geometry",
                            "1.0",
                            scope,
                            maps,
                            new MinimumClusterPersistencePolicy(
                                "p",
                                1,
                                0,
                                0),
                            new BoundaryVarianceClusterStabilityMetric(),
                            new PersistentClusterRecognitionPolicy()));

                var firstFixture =
                    new H2TestFixture();

                var secondFixture =
                    new H2TestFixture();

                var now =
                    DateTimeOffset.UtcNow;

                var firstEvaluation =
                    firstFixture.Eval(
                        now,
                        ("x", 0.1),
                        ("y", 0));

                var secondEvaluation =
                    secondFixture.Eval(
                        now,
                        ("x", 9.9),
                        ("y", 0));

                var first =
                    discoveries.Geometry.Observe(
                            firstEvaluation,
                            firstKey,
                            "gobbo-escape-history")
                        .Recognition;

                var second =
                    discoveries.Geometry.Observe(
                            secondEvaluation,
                            secondKey,
                            "gobbo-failure-history")
                        .Recognition;

                var population =
                    discoveries.Narratives.CreatePopulation(
                        geometryRegistration.Id,
                        new List<
                            IDiscoveryClusterRecognitionResult>
                        {
                            first,
                            second
                        });

                return new Fixture(
                    discoveries,
                    population,
                    scope,
                    firstKey,
                    secondKey,
                    first,
                    second,
                    firstFixture);
            }

            public IDiscoveryCompetingNarrativesResult
                Competing()
            {
                var relationship =
                    new DiscoveryNarrativeRelationshipDefinition(
                        Guid.NewGuid(),
                        "competing",
                        FirstKey,
                        SecondKey,
                        "CompetingOutcome");

                var profile =
                    new DiscoveryNarrativeProfile(
                        Guid.NewGuid(),
                        "gobbo-narratives",
                        "1.0",
                        Scope,
                        new RelationshipKeyNarrativeCompositionPolicy(
                            "ReinforcingOutcome"),
                        new RecognitionCentroidDistanceNarrativeMetric(),
                        new List<
                            IDiscoveryNarrativeRelationshipDefinition>
                        {
                            relationship
                        });

                var registration =
                    Discoveries.Narratives.Register(
                        profile);

                return Discoveries.Outcomes.Relate(
                    Discoveries.Narratives.Compose(
                        Population,
                        registration.Id));
            }

            public IDiscoveryVectorEvaluationSeries
                Series()
            {
                var evaluation =
                    First.Persistence
                        .Observations[0]
                        .Relationship
                        .Evaluation;

                return new DiscoveryVectorEvaluationSeries(
                    Scope,
                    new List<IDiscoveryVectorInput>
                    {
                        _firstFixture.Input
                    },
                    new List<IDiscoveryVectorEvaluation>
                    {
                        evaluation
                    });
            }

            public IReadOnlyList<IDiscoveryCluster>
                Clusters(
                    IDiscoveryVectorSeries series)
            {
                return new List<IDiscoveryCluster>
                {
                    new TestCluster(
                        new List<IDiscoveryVector>
                        {
                            series.Vectors[0]
                        })
                };
            }

            public IDiscoveryTruthCongealmentResult
                CongealedTruth()
            {
                var competing =
                    Competing();

                var evaluationSeries =
                    Series();

                var series =
                    (IDiscoveryVectorSeries)
                        evaluationSeries;

                var clusters =
                    Clusters(
                        series);

                var policy =
                    new TestTruthPolicy(
                        true);

                var registration =
                    Discoveries
                        .Outcomes
                        .Truths
                        .Register(
                            new DiscoveryTruthProfile(
                                Guid.NewGuid(),
                                "gobbo-truth",
                                "1.0",
                                Scope,
                                policy));

                return Discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        competing,
                        registration.Id,
                        series,
                        clusters);
            }

            private static IDiscoveryClusterMapping Map(
                Guid profileId,
                string slug,
                string key,
                double x)
            {
                return new DiscoveryClusterMapping(
                    Guid.NewGuid(),
                    slug,
                    profileId,
                    "gobbo-geometry",
                    "1.0",
                    key,
                    "hypersphere",
                    new Dictionary<string, double>
                    {
                        ["x"] = x,
                        ["y"] = 0
                    },
                    1);
            }
        }

        private sealed class TestCluster :
            IDiscoveryCluster
        {
            public TestCluster(
                IReadOnlyList<IDiscoveryVector> members)
            {
                Members =
                    members;

                Centroid =
                    members[0];

                Density =
                    1;

                Dispersion =
                    0;

                Coherence =
                    1;
            }

            public IReadOnlyList<IDiscoveryVector>
                Members { get; }

            public IDiscoveryVectorPoint
                Centroid { get; }

            public double Density { get; }

            public double Dispersion { get; }

            public double Coherence { get; }
        }

        private sealed class TestTruth :
            ITruth
        {
            public TestTruth(
                IDiscoveryVectorSeries series,
                IReadOnlyList<IDiscoveryCluster> clusters)
            {
                Id =
                    Guid.NewGuid();

                Slug =
                    "gobbo-morning-truth";

                Name =
                    "MorningArrivesBeforeSafety";

                Series =
                    series;

                Clusters =
                    clusters;

                Discoveries =
                    new List<IDiscovery>();

                Stability =
                    1;

                Confidence =
                    1;

                CongealedAt =
                    DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }

            public string Slug { get; }

            public string Name { get; }

            public object? Value =>
                true;

            public IDiscoveryVectorSeries
                Series { get; }

            public IReadOnlyList<IDiscoveryCluster>
                Clusters { get; }

            public IReadOnlyList<IDiscovery>
                Discoveries { get; }

            public double Stability { get; }

            public double Confidence { get; }

            public DateTimeOffset
                CongealedAt { get; }
        }

        private sealed class TestTruthPolicy :
            IIdentifiedTruthCongealmentPolicy
        {
            private readonly bool
                _can;

            public TestTruthPolicy(
                bool can)
            {
                _can =
                    can;

                Id =
                    Guid.NewGuid();
            }

            public Guid Id { get; }

            public string Slug =>
                "test-truth-policy";

            public bool CongealCalled
            {
                get;
                private set;
            }

            public IDiscoveryCompetingNarrativesResult?
                CanCongealNarratives { get; private set; }

            public IReadOnlyList<IDiscoveryCluster>?
                CanCongealClusters { get; private set; }

            public IDiscoveryVectorSeries?
                CanCongealSeries { get; private set; }

            public IDiscoveryCompetingNarrativesResult?
                CongealNarratives { get; private set; }

            public IReadOnlyList<IDiscoveryCluster>?
                CongealClusters { get; private set; }

            public IDiscoveryVectorSeries?
                CongealSeries { get; private set; }

            public bool CanCongeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CanCongealNarratives =
                    narratives;

                CanCongealClusters =
                    clusters;

                CanCongealSeries =
                    series;

                return _can;
            }

            public ITruth Congeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CongealCalled =
                    true;

                CongealNarratives =
                    narratives;

                CongealClusters =
                    clusters;

                CongealSeries =
                    series;

                return new TestTruth(
                    series,
                    clusters);
            }
        }

        /// <summary>
        /// Consumer policy proving that narrative context may
        /// materially inform the consumer-owned Truth decision.
        ///
        /// Discoveries supplies the context. This policy decides
        /// what that context means.
        /// </summary>
        private sealed class NarrativeSensitiveTruthPolicy :
            IIdentifiedTruthCongealmentPolicy
        {
            private readonly
                IDiscoveryCompetingNarrativesResult
                _acceptedNarratives;

            public NarrativeSensitiveTruthPolicy(
                IDiscoveryCompetingNarrativesResult acceptedNarratives)
            {
                _acceptedNarratives =
                    acceptedNarratives ??
                    throw new ArgumentNullException(
                        nameof(acceptedNarratives));

                Id =
                    Guid.NewGuid();
            }

            public Guid Id { get; }

            public string Slug =>
                "narrative-sensitive-truth-policy";

            public bool CongealCalled
            {
                get;
                private set;
            }

            public IDiscoveryCompetingNarrativesResult?
                CanCongealNarratives { get; private set; }

            public IReadOnlyList<IDiscoveryCluster>?
                CanCongealClusters { get; private set; }

            public IDiscoveryVectorSeries?
                CanCongealSeries { get; private set; }

            public IDiscoveryCompetingNarrativesResult?
                CongealNarratives { get; private set; }

            public IReadOnlyList<IDiscoveryCluster>?
                CongealClusters { get; private set; }

            public IDiscoveryVectorSeries?
                CongealSeries { get; private set; }

            public bool CanCongeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CanCongealNarratives =
                    narratives;

                CanCongealClusters =
                    clusters;

                CanCongealSeries =
                    series;

                // This decision belongs to the consumer.
                //
                // The exact same clusters and vector series can
                // produce a different decision when the consumer is
                // given a different narrative context.
                return ReferenceEquals(
                    narratives,
                    _acceptedNarratives);
            }

            public ITruth Congeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CongealCalled =
                    true;

                CongealNarratives =
                    narratives;

                CongealClusters =
                    clusters;

                CongealSeries =
                    series;

                return new TestTruth(
                    series,
                    clusters);
            }

            public void ResetCallEvidence()
            {
                CongealCalled =
                    false;

                CanCongealNarratives =
                    null;

                CanCongealClusters =
                    null;

                CanCongealSeries =
                    null;

                CongealNarratives =
                    null;

                CongealClusters =
                    null;

                CongealSeries =
                    null;
            }
        }

        private sealed class TestConsequence :
            IConsequence
        {
            public TestConsequence(
                ITruth truth)
            {
                Id =
                    Guid.NewGuid();

                Slug =
                    "gobbo-may-turn-stone";

                Name =
                    "GobboMayTurnToStone";

                Truths =
                    new List<ITruth>
                    {
                        truth
                    };

                Magnitude =
                    1;

                Confidence =
                    1;

                ProjectedAt =
                    DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }

            public string Slug { get; }

            public string Name { get; }

            public object? Value =>
                true;

            public IReadOnlyList<ITruth>
                Truths { get; }

            public double Magnitude { get; }

            public double Confidence { get; }

            public DateTimeOffset
                ProjectedAt { get; }
        }

        private sealed class TestConsequencePolicy :
            IIdentifiedConsequenceProjectionPolicy
        {
            private readonly bool
                _project;

            public TestConsequencePolicy(
                bool project)
            {
                _project =
                    project;

                Id =
                    Guid.NewGuid();
            }

            public Guid Id { get; }

            public string Slug =>
                "test-consequence-policy";

            public bool ProjectCalled
            {
                get;
                private set;
            }

            public IReadOnlyList<IConsequence> Project(
                IReadOnlyList<ITruth> truths)
            {
                ProjectCalled =
                    true;

                if (!_project)
                {
                    return new List<IConsequence>();
                }

                if (truths.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                return new List<IConsequence>
                {
                    new TestConsequence(
                        truths[0])
                };
            }
        }
    }
}