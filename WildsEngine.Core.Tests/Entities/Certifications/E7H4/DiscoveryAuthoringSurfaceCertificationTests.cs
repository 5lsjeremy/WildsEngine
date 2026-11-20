using System;
using System.Collections.Generic;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Consequences;
using WildsEngine.Discoveries.Narratives;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Truths;
using WildsEngine.Discoveries.Truths;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryAuthoringSurfaceCertificationTests
    {
        [Fact]
        public void ConvenienceAuthoringRemovesBookkeepingWithoutRemovingConsumerMeaning()
        {
            const string scope = "authoring-certification";

            var truthPolicy = new TestTruthPolicy();
            var consequencePolicy = new TestConsequencePolicy();

            var truthProfile = new DiscoveryTruthProfile(
                "authoring-truth",
                scope,
                truthPolicy);

            var consequenceProfile = new DiscoveryConsequenceProfile(
                "authoring-consequence",
                scope,
                consequencePolicy);

            var narrativePolicy = new RelationshipKeyNarrativeCompositionPolicy(
                "ReinforcingKnowledge");

            var narrativeMetric = new RecognitionCentroidDistanceNarrativeMetric();

            var narrativeProfile = new DiscoveryNarrativeProfile(
                "authoring-narrative",
                scope,
                narrativePolicy,
                narrativeMetric,
                new List<IDiscoveryNarrativeRelationshipDefinition>());

            Assert.NotEqual(Guid.Empty, truthProfile.Id);
            Assert.Equal("1.0", truthProfile.Version);
            Assert.Equal(scope, truthProfile.Scope);
            Assert.Same(truthPolicy, truthProfile.CongealmentPolicy);

            Assert.NotEqual(Guid.Empty, consequenceProfile.Id);
            Assert.Equal("1.0", consequenceProfile.Version);
            Assert.Equal(scope, consequenceProfile.Scope);
            Assert.Same(consequencePolicy, consequenceProfile.ProjectionPolicy);

            Assert.NotEqual(Guid.Empty, narrativeProfile.Id);
            Assert.Equal("1.0", narrativeProfile.Version);
            Assert.Equal(scope, narrativeProfile.Scope);
            Assert.Same(narrativePolicy, narrativeProfile.CompositionPolicy);
            Assert.Same(narrativeMetric, narrativeProfile.Metric);

            var entity = new TestEntity("kid-5");

            Assert.Equal("kid-5", DiscoveryHistoryKey.ForEntity(entity));
            Assert.Equal("kid-5", DiscoveryHistoryKey.ForSubject(" kid-5 "));
            Assert.Equal("kids-1-through-4", DiscoveryHistoryKey.ForPopulation(" kids-1-through-4 "));
            Assert.Equal("kid-5:nearby-group", DiscoveryHistoryKey.ForPerspective(" kid-5 ", " nearby-group "));
        }

        private sealed class TestTruthPolicy : IIdentifiedTruthCongealmentPolicy
        {
            public TestTruthPolicy()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; }
            public string Slug => "authoring-truth-policy";

            public bool CanCongeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                return false;
            }

            public ITruth Congeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                throw new InvalidOperationException(
                    "This authoring certification never authorizes Truth congealment.");
            }
        }

        private sealed class TestConsequencePolicy : IIdentifiedConsequenceProjectionPolicy
        {
            public TestConsequencePolicy()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; }
            public string Slug => "authoring-consequence-policy";

            public IReadOnlyList<IConsequence> Project(
                IReadOnlyList<ITruth> truths)
            {
                return new List<IConsequence>();
            }
        }
    }
}
