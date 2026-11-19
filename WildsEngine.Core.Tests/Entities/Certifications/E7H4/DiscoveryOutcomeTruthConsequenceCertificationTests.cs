using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Truths;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Consequences;
using WildsEngine.Discoveries.Narratives;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Discoveries.Truths;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryOutcomeTruthConsequenceCertificationTests
    {
        [Fact]
        public void H4C_RetainsCrossGroupRejectedDecisionAndDesignerDefinition()
        {
            var fixture = Fixture.Create();
            var relationship = new DiscoveryNarrativeRelationshipDefinition(
                Guid.NewGuid(), "competing", fixture.FirstKey, fixture.SecondKey, "CompetingOutcome");
            var profile = new DiscoveryNarrativeProfile(
                Guid.NewGuid(), "gobbo-narratives", "1.0", fixture.Scope,
                new RelationshipKeyNarrativeCompositionPolicy("ReinforcingOutcome"),
                new RecognitionCentroidDistanceNarrativeMetric(),
                new List<IDiscoveryNarrativeRelationshipDefinition> { relationship });
            var registration = fixture.Discoveries.Narratives.Register(profile);
            var composition = fixture.Discoveries.Narratives.Compose(fixture.Population, registration.Id);
            Assert.Equal(2, composition.Groups.Count);
            Assert.Single(composition.Decisions);
            Assert.False(composition.Decisions[0].CanCompose);
            Assert.Same(relationship, composition.Decisions[0].Relationship);

            var competing = fixture.Discoveries.Outcomes.Relate(composition);
            Assert.Single(competing.GroupRelationships);
            var relation = competing.GroupRelationships[0];
            Assert.Single(relation.Decisions);
            Assert.Same(composition.Decisions[0], relation.Decisions[0]);
            Assert.Single(relation.Definitions);
            Assert.Same(relationship, relation.Definitions[0]);
        }

        [Fact]
        public void H5_RefusedCongealmentProducesNoTruthAndNeverCallsCongeal()
        {
            var fixture = Fixture.Create();
            var competing = fixture.Competing();
            var series = fixture.Series();
            var clusters = fixture.Clusters(series);
            var policy = new TestTruthPolicy(false);
            var profile = new DiscoveryTruthProfile(Guid.NewGuid(), "gobbo-truth", "1.0", fixture.Scope, policy);
            var registration = fixture.Discoveries.Outcomes.Truths.Register(profile);

            var result = fixture.Discoveries.Outcomes.Truths.Congeal(
                competing, registration.Id, series, clusters);

            Assert.False(result.Congealed);
            Assert.Empty(result.Truths);
            Assert.False(policy.CongealCalled);
            Assert.Same(series, result.Series);
            Assert.Same(competing, result.Narratives);
            Assert.Equal(policy.Id, result.Registration.CongealmentPolicyId);
        }

        [Fact]
        public void H5_ApprovedCongealmentRetainsExactSeriesClustersAndPolicyIdentity()
        {
            var fixture = Fixture.Create();
            var competing = fixture.Competing();
            var series = fixture.Series();
            var clusters = fixture.Clusters(series);
            var policy = new TestTruthPolicy(true);
            var profile = new DiscoveryTruthProfile(Guid.NewGuid(), "gobbo-truth", "1.0", fixture.Scope, policy);
            var registration = fixture.Discoveries.Outcomes.Truths.Register(profile);

            var result = fixture.Discoveries.Outcomes.Truths.Congeal(
                competing, registration.Id, series, clusters);

            Assert.True(result.Congealed);
            Assert.Single(result.Truths);
            Assert.True(policy.CongealCalled);
            var truth = result.Truths[0];
            Assert.Same(series, truth.Series);
            Assert.Equal(clusters.Count, truth.Clusters.Count);
            for (var i = 0; i < clusters.Count; i++) Assert.Same(clusters[i], truth.Clusters[i]);
            Assert.Equal(policy.Id, result.Registration.CongealmentPolicyId);
        }

        [Fact]
        public void H6_EmptyConsumerProjectionIsValid()
        {
            var fixture = Fixture.Create();
            var truthResult = fixture.CongealedTruth();
            var policy = new TestConsequencePolicy(project: false);
            var profile = new DiscoveryConsequenceProfile(
                Guid.NewGuid(), "gobbo-consequence", "1.0", fixture.Scope, policy);
            var registration = fixture.Discoveries.Outcomes.Consequences.Register(profile);

            var result = fixture.Discoveries.Outcomes.Consequences.Project(truthResult, registration.Id);

            Assert.Empty(result.Consequences);
            Assert.Same(truthResult, result.TruthResult);
            Assert.Equal(policy.Id, result.Registration.ProjectionPolicyId);
        }

        [Fact]
        public void H6_ProjectedConsequenceRetainsExactTruth()
        {
            var fixture = Fixture.Create();
            var truthResult = fixture.CongealedTruth();
            var policy = new TestConsequencePolicy(project: true);
            var profile = new DiscoveryConsequenceProfile(
                Guid.NewGuid(), "gobbo-consequence", "1.0", fixture.Scope, policy);
            var registration = fixture.Discoveries.Outcomes.Consequences.Register(profile);

            var result = fixture.Discoveries.Outcomes.Consequences.Project(truthResult, registration.Id);

            Assert.Single(result.Consequences);
            var consequence = result.Consequences[0];
            Assert.Equal("GobboMayTurnToStone", consequence.Name);
            Assert.Single(consequence.Truths);
            Assert.Same(truthResult.Truths[0], consequence.Truths[0]);
            Assert.Equal(policy.Id, result.Registration.ProjectionPolicyId);
        }

        private sealed class Fixture
        {
            private Fixture(WildsDiscoveries discoveries, IDiscoveryRecognizedGeometryPopulation population,
                string scope, string firstKey, string secondKey,
                IDiscoveryClusterRecognitionResult first, IDiscoveryClusterRecognitionResult second)
            { Discoveries=discoveries; Population=population; Scope=scope; FirstKey=firstKey; SecondKey=secondKey; First=first; Second=second; }
            public WildsDiscoveries Discoveries { get; }
            public IDiscoveryRecognizedGeometryPopulation Population { get; }
            public string Scope { get; }
            public string FirstKey { get; }
            public string SecondKey { get; }
            public IDiscoveryClusterRecognitionResult First { get; }
            public IDiscoveryClusterRecognitionResult Second { get; }

            public static Fixture Create()
            {
                const string scope="gobbo-temple-run", a="EscapeStillPossible", b="FailureBecomingLikely";
                var d=new WildsDiscoveries();var pid=Guid.NewGuid();
                var maps=new List<IDiscoveryClusterMapping>{Map(pid,"escape",a,0),Map(pid,"failure",b,10)};
                var gr=d.Geometry.Register(new DiscoveryGeometryProfile(pid,"gobbo-geometry","1.0",scope,maps,
                    new MinimumClusterPersistencePolicy("p",1,0,0),new BoundaryVarianceClusterStabilityMetric(),new PersistentClusterRecognitionPolicy()));
                var va=new H2TestFixture();var vb=new H2TestFixture();
                var ra=d.Geometry.Observe(va.Eval(DateTimeOffset.UtcNow,("x",.1),("y",0)),a).Recognition;
                var rb=d.Geometry.Observe(vb.Eval(DateTimeOffset.UtcNow,("x",9.9),("y",0)),b).Recognition;
                var pop=d.Narratives.CreatePopulation(gr.Id,new List<IDiscoveryClusterRecognitionResult>{ra,rb});
                return new Fixture(d,pop,scope,a,b,ra,rb);
            }
            public IDiscoveryCompetingNarrativesResult Competing()
            {
                var rel=new DiscoveryNarrativeRelationshipDefinition(Guid.NewGuid(),"competing",FirstKey,SecondKey,"CompetingOutcome");
                var profile=new DiscoveryNarrativeProfile(Guid.NewGuid(),"gobbo-narratives","1.0",Scope,
                    new RelationshipKeyNarrativeCompositionPolicy("ReinforcingOutcome"),new RecognitionCentroidDistanceNarrativeMetric(),
                    new List<IDiscoveryNarrativeRelationshipDefinition>{rel});
                var nr=Discoveries.Narratives.Register(profile);
                return Discoveries.Outcomes.Relate(Discoveries.Narratives.Compose(Population,nr.Id));
            }
            public IDiscoveryVectorSeries Series()
            {
                return new TestSeries(Scope,new List<IDiscoveryVector>{First.Persistence.Observations[0].Relationship.Evaluation.Vector});
            }
            public IReadOnlyList<IDiscoveryCluster> Clusters(IDiscoveryVectorSeries series)
            {
                return new List<IDiscoveryCluster>{new TestCluster(new List<IDiscoveryVector>{series.Vectors[0]})};
            }
            public IDiscoveryTruthCongealmentResult CongealedTruth()
            {
                var competing=Competing();var series=Series();var clusters=Clusters(series);var policy=new TestTruthPolicy(true);
                var reg=Discoveries.Outcomes.Truths.Register(new DiscoveryTruthProfile(Guid.NewGuid(),"gobbo-truth","1.0",Scope,policy));
                return Discoveries.Outcomes.Truths.Congeal(competing,reg.Id,series,clusters);
            }
            private static IDiscoveryClusterMapping Map(Guid id,string slug,string key,double x)=>new DiscoveryClusterMapping(Guid.NewGuid(),slug,id,"gobbo-geometry","1.0",key,"hypersphere",new Dictionary<string,double>{{"x",x},{"y",0}},1);
        }

        private sealed class TestSeries : IDiscoveryVectorSeries
        {
            public TestSeries(string scope,IReadOnlyList<IDiscoveryVector> vectors){Scope=scope;Vectors=vectors;Inputs=new List<IDiscoveryVectorInput>();}
            public string Scope{get;} public IReadOnlyList<IDiscoveryVectorInput> Inputs{get;} public IReadOnlyList<IDiscoveryVector> Vectors{get;}
            public IDiscoveryVector? Current=>Vectors.Count==0?null:Vectors[Vectors.Count-1]; public IDiscoveryVector? Previous=>Vectors.Count<2?null:Vectors[Vectors.Count-2];
        }
        private sealed class TestCluster : IDiscoveryCluster
        {
            public TestCluster(IReadOnlyList<IDiscoveryVector> members){Members=members;Centroid=members[0];Density=1;Dispersion=0;Coherence=1;}
            public IReadOnlyList<IDiscoveryVector> Members{get;}public IDiscoveryVectorPoint Centroid{get;}public double Density{get;}public double Dispersion{get;}public double Coherence{get;}
        }
        private sealed class TestTruth : ITruth
        {
            public TestTruth(IDiscoveryVectorSeries series,IReadOnlyList<IDiscoveryCluster> clusters){Id=Guid.NewGuid();Slug="gobbo-morning-truth";Name="MorningArrivesBeforeSafety";Series=series;Clusters=clusters;Discoveries=new List<IDiscovery>();Stability=1;Confidence=1;CongealedAt=DateTimeOffset.UtcNow;}
            public Guid Id{get;}public string Slug{get;}public string Name{get;}public object? Value=>true;public IDiscoveryVectorSeries Series{get;}public IReadOnlyList<IDiscoveryCluster> Clusters{get;}public IReadOnlyList<IDiscovery> Discoveries{get;}public double Stability{get;}public double Confidence{get;}public DateTimeOffset CongealedAt{get;}
        }
        private sealed class TestTruthPolicy : IIdentifiedTruthCongealmentPolicy
        {
            private readonly bool _can; public TestTruthPolicy(bool can){_can=can;Id=Guid.NewGuid();}public Guid Id{get;}public string Slug=>"test-truth-policy";public bool CongealCalled{get;private set;}
            public bool CanCongeal(IReadOnlyList<IDiscoveryCluster> clusters,IDiscoveryVectorSeries series)=>_can;
            public ITruth Congeal(IReadOnlyList<IDiscoveryCluster> clusters,IDiscoveryVectorSeries series){CongealCalled=true;return new TestTruth(series,clusters);}
        }
        private sealed class TestConsequence : IConsequence
        {
            public TestConsequence(ITruth truth){Id=Guid.NewGuid();Slug="gobbo-may-turn-stone";Name="GobboMayTurnToStone";Truths=new List<ITruth>{truth};Magnitude=1;Confidence=1;ProjectedAt=DateTimeOffset.UtcNow;}
            public Guid Id{get;}public string Slug{get;}public string Name{get;}public object? Value=>true;public IReadOnlyList<ITruth> Truths{get;}public double Magnitude{get;}public double Confidence{get;}public DateTimeOffset ProjectedAt{get;}
        }
        private sealed class TestConsequencePolicy : IIdentifiedConsequenceProjectionPolicy
        {
            private readonly bool _project;public TestConsequencePolicy(bool project){_project=project;Id=Guid.NewGuid();}public Guid Id{get;}public string Slug=>"test-consequence-policy";
            public IReadOnlyList<IConsequence> Project(IReadOnlyList<ITruth> truths){if(!_project)return new List<IConsequence>();if(truths.Count==0)throw new InvalidOperationException();return new List<IConsequence>{new TestConsequence(truths[0])};}
        }
    }
}
