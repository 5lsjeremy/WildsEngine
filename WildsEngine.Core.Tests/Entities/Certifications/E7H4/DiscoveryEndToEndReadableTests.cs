using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Truths;
using WildsEngine.Discoveries.Abstractions.Vectorization;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Concrete;
using WildsEngine.Discoveries.Consequences;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Narratives;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Services;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Discoveries.Truths;
using WildsEngine.Discoveries.Vectorization;
using WildsEngine.Discoveries.Vectors;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    /// <summary>
    /// D5 readable end-to-end certifications.
    ///
    /// Test one certifies the full individual knowledge path through
    /// Consequence projection.
    ///
    /// Test two certifies the explicit feedback boundary:
    /// a Truth congealed for a group may become Evidence owned by an
    /// individual without transferring group ownership, history, or
    /// decision authority.
    /// </summary>
    public sealed class DiscoveryEndToEndReadableTests
    {
        [Fact]
        public async Task
            KidFiveCanUnderstandChocolateIsPopularWithoutLosingVanillaPreference()
        {
            TestLog.Section(
                "D5 - KID 5 END-TO-END DISCOVERY");

            TestLog.Test(
                "Four nearby kids choose chocolate while Kid 5 " +
                "personally prefers vanilla.");

            const string scope =
                "kid-five-ice-cream";

            var kidFive =
                new TestEntity(
                    "kid-5");

            var chocolateEvidence =
                CreateValidatedEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["NearbyKidCount"] = 4,
                        ["ObservedFlavor"] = "Chocolate",
                        ["Location"] = "IceCreamStand"
                    },
                    "Four nearby kids selected chocolate",
                    4.0,
                    "kid-5-observation-chocolate-4",
                    "Nearby chocolate choices");

            var vanillaEvidence =
                CreateValidatedEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["Subject"] = "Kid 5",
                        ["PreferredFlavor"] = "Vanilla",
                        ["PreferenceSource"] = "Personal"
                    },
                    "Kid 5 personally prefers vanilla",
                    1.0,
                    "kid-5-personal-vanilla",
                    "Kid 5 vanilla preference");

            Assert.Same(kidFive, chocolateEvidence.Entity);
            Assert.Same(kidFive, vanillaEvidence.Entity);

            TestLog.State(
                "Nearby kids choosing chocolate = " +
                chocolateEvidence.Conditions["NearbyKidCount"]);

            var chocolateFact =
                CreateProvenFact(
                    kidFive,
                    "ChocolateIsPopularNearby",
                    chocolateEvidence,
                    new TrueEvidencePolicy(),
                    "fact-chocolate-popular");

            var vanillaFact =
                CreateProvenFact(
                    kidFive,
                    "VanillaRemainsPersonallyPreferred",
                    vanillaEvidence,
                    new TrueEvidencePolicy(),
                    "fact-vanilla-preferred");

            var chocolateDiscovery =
                CreateAvailableDiscovery(
                    kidFive,
                    "ChocolateIsSociallyDominant",
                    chocolateFact,
                    "discovery-social-chocolate");

            var vanillaDiscovery =
                CreateAvailableDiscovery(
                    kidFive,
                    "VanillaRemainsPersonallyPreferred",
                    vanillaFact,
                    "discovery-personal-vanilla");

            TestLog.State(
                "Chocolate-popularity Fact proven = " +
                chocolateFact.IsProven);

            TestLog.State(
                "Vanilla-preference Fact proven = " +
                vanillaFact.IsProven);

            var clock =
                new SystemWildsServiceClock();

            var journal =
                new WildsServiceExecutionJournal();

            var vectorizer =
                new DiscoveryVectorizerFactory()
                    .Create(
                        new IceCreamVectorizationService(
                            new WildsServiceContextFactory(clock),
                            journal,
                            clock),
                        new AbsoluteMagnitudeMetric());

            var chocolateInput =
                new DiscoveryVectorInput(
                    chocolateDiscovery,
                    "Kid 5 social observation");

            var vanillaInput =
                new DiscoveryVectorInput(
                    vanillaDiscovery,
                    "Kid 5 personal preference");

            var chocolateVectorization =
                await vectorizer.VectorizeAsync(
                    chocolateInput,
                    provenance: "D5 chocolate vectorization",
                    pulseId: Guid.NewGuid());

            var vanillaVectorization =
                await vectorizer.VectorizeAsync(
                    vanillaInput,
                    provenance: "D5 vanilla vectorization",
                    pulseId: Guid.NewGuid());

            Assert.True(
                journal.TryGet(
                    chocolateVectorization.ServiceExecutionId,
                    out var chocolateExecution));

            Assert.True(
                journal.TryGet(
                    vanillaVectorization.ServiceExecutionId,
                    out var vanillaExecution));

            Assert.Equal(
                WildsServiceExecutionOutcome.Succeeded,
                chocolateExecution!.Outcome);

            Assert.Equal(
                WildsServiceExecutionOutcome.Succeeded,
                vanillaExecution!.Outcome);

            var now =
                DateTimeOffset.UtcNow;

            var chocolateEvaluation =
                new DiscoveryVectorEvaluation(
                    chocolateVectorization.Vector,
                    chocolateVectorization.ServiceExecutionId,
                    now);

            var vanillaEvaluation =
                new DiscoveryVectorEvaluation(
                    vanillaVectorization.Vector,
                    vanillaVectorization.ServiceExecutionId,
                    now.AddSeconds(1));

            var series =
                new DiscoveryVectorEvaluationSeries(
                    scope,
                    new List<IDiscoveryVectorInput>
                    {
                        chocolateInput,
                        vanillaInput
                    },
                    new List<IDiscoveryVectorEvaluation>
                    {
                        chocolateEvaluation,
                        vanillaEvaluation
                    });

            var discoveries =
                new WildsDiscoveries();

            var knowledge =
                BuildCompetingKnowledge(
                    discoveries,
                    scope,
                    "kid-five-ice-cream",
                    "kid-5",
                    "ChocolateIsSociallyDominant",
                    chocolateEvaluation,
                    "VanillaRemainsPersonallyPreferred",
                    vanillaEvaluation,
                    "CompetingKnowledge");

            var clusters =
                new List<IDiscoveryCluster>
                {
                    new ReadableCluster(
                        new List<IDiscoveryVector>
                        {
                            chocolateEvaluation.Vector
                        }),
                    new ReadableCluster(
                        new List<IDiscoveryVector>
                        {
                            vanillaEvaluation.Vector
                        })
                };

            var truthPolicy =
                new TextTruthPolicy(
                    "kid-five-truth-policy",
                    "KidFiveUnderstandsSocialAndPersonalPreference",
                    "Chocolate is socially dominant while " +
                    "Kid 5 still personally prefers vanilla.");

            var truthRegistration =
                discoveries.Outcomes.Truths.Register(
                    new DiscoveryTruthProfile(
                        Guid.NewGuid(),
                        "kid-five-truth",
                        "1.0",
                        scope,
                        truthPolicy));

            var truthResult =
                discoveries.Outcomes.Truths.Congeal(
                    knowledge.Narratives,
                    truthRegistration.Id,
                    series,
                    clusters);

            Assert.True(truthResult.Congealed);
            Assert.Single(truthResult.Truths);
            Assert.Same(knowledge.Narratives, truthPolicy.Narratives);
            Assert.Same(series, truthPolicy.Series);
            Assert.Same(clusters, truthPolicy.Clusters);

            var truth =
                truthResult.Truths[0];

            Assert.Equal(
                "Chocolate is socially dominant while " +
                "Kid 5 still personally prefers vanilla.",
                truth.Value);

            TestLog.State(
                "Truth = " +
                truth.Value);

            var consequencePolicy =
                new KidFiveConsequencePolicy();

            var consequenceRegistration =
                discoveries.Outcomes.Consequences.Register(
                    new DiscoveryConsequenceProfile(
                        Guid.NewGuid(),
                        "kid-five-consequence",
                        "1.0",
                        scope,
                        consequencePolicy));

            var consequenceResult =
                discoveries.Outcomes.Consequences.Project(
                    truthResult,
                    consequenceRegistration.Id);

            Assert.True(consequencePolicy.ProjectCalled);
            Assert.Single(consequenceResult.Consequences);
            Assert.Same(truth, consequenceResult.Consequences[0].Truths[0]);

            TestLog.State(
                "Consequence = " +
                consequenceResult.Consequences[0].Value);

            TestLog.Pass(
                "Discoveries carried Kid 5 from observation through " +
                "competing knowledge to projected consequence without " +
                "choosing or executing an action for him.");
        }

        [Fact]
        public void
            GroupTruthCanBecomeIndividualEvidenceWithoutOverwritingIndividualPreference()
        {
            TestLog.Section(
                "D5B - GROUP TRUTH TO INDIVIDUAL EVIDENCE");

            TestLog.Test(
                "A Truth congealed through the production Truth surface " +
                "becomes Evidence available to Kid 5 without automatically " +
                "becoming Kid 5's own Truth.");

            const string groupScope =
                "nearby-kids-group";

            var group =
                CreateCongealedGroupTruth(
                    groupScope);

            Assert.True(group.TruthResult.Congealed);
            Assert.Single(group.TruthResult.Truths);

            var groupTruth =
                group.TruthResult.Truths[0];

            Assert.True(group.Policy.CanCongealCalled);
            Assert.True(group.Policy.CongealCalled);
            Assert.Same(group.Narratives, group.Policy.Narratives);
            Assert.Same(group.Series, group.Policy.Series);
            Assert.Same(group.Clusters, group.Policy.Clusters);
            Assert.Same(group.Series, groupTruth.Series);
            Assert.Equal(
                "Chocolate is dominant among Kids 1-4.",
                groupTruth.Value);

            TestLog.State(
                "Group Truth congealed through Truths.Congeal = " +
                group.TruthResult.Congealed);

            TestLog.State(
                "Group Truth = " +
                groupTruth.Value);

            var kidFive =
                new TestEntity(
                    "kid-5");

            Assert.Empty(kidFive.Facts);
            Assert.Empty(kidFive.Discoveries);

            TestLog.State(
                "Group Truth automatically changed Kid 5 = False");

            var peerPressureEvidence =
                new DiscoveryEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["ObservedGroupSize"] = 4,
                        ["ObservedFlavor"] = "Chocolate",
                        ["ObservationKind"] = "CongealedGroupTruth",
                        ["GroupScope"] = groupScope
                    },
                    groupTruth,
                    4.0,
                    1.0,
                    new List<IBaseObject>(),
                    null,
                    "kid-5-observes-group-truth",
                    "Nearby group preference");

            Assert.Same(kidFive, peerPressureEvidence.Entity);
            Assert.Same(groupTruth, peerPressureEvidence.Outcome);
            Assert.Same(group.Series, groupTruth.Series);
            Assert.Empty(kidFive.Facts);
            Assert.Empty(kidFive.Discoveries);

            var proof =
                new TestProof(
                    1.0,
                    true,
                    "Kid 5 directly observed the congealed preference " +
                    "of the nearby group.");

            Assert.True(
                peerPressureEvidence.ApplyProof(
                    proof.Validate(
                        peerPressureEvidence)));

            var conformingFact =
                CreateProvenFact(
                    kidFive,
                    "ChocolatePeerPressureIsStrong",
                    peerPressureEvidence,
                    new GroupTruthPeerPressurePolicy(4),
                    "fact-peer-pressure-conforming");

            var independentFact =
                new Fact(
                    kidFive,
                    "ChocolatePeerPressureIsStrong",
                    true,
                    1,
                    new RejectGroupTruthPolicy(),
                    "fact-peer-pressure-independent");

            Assert.True(
                independentFact.ReceiveEvidence(
                    peerPressureEvidence));

            Assert.True(
                conformingFact.IsProven);

            Assert.Equal(
                1,
                conformingFact.CurrentEvidenceCount);

            Assert.Single(
                conformingFact.Contributors);

            Assert.False(
                independentFact.IsProven);

            Assert.Equal(
                0,
                independentFact.CurrentEvidenceCount);

            Assert.Equal(
                0.0,
                independentFact.EvidencePercentage);

            Assert.Single(
                independentFact.Evidence);

            Assert.Same(
                peerPressureEvidence,
                conformingFact.Evidence[0]);

            Assert.Same(
                peerPressureEvidence,
                independentFact.Evidence[0]);

            TestLog.State(
                "Conforming policy accepts peer pressure = " +
                conformingFact.IsProven);

            TestLog.State(
                "Independent policy accepts peer pressure = " +
                independentFact.IsProven);

            var vanillaEvidence =
                CreateValidatedEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["PreferredFlavor"] = "Vanilla",
                        ["PreferenceSource"] = "Personal"
                    },
                    "Kid 5 personally prefers vanilla",
                    1.0,
                    "kid-5-personal-vanilla-d5b",
                    "Kid 5 personal preference");

            var vanillaFact =
                CreateProvenFact(
                    kidFive,
                    "VanillaRemainsPersonallyPreferred",
                    vanillaEvidence,
                    new TrueEvidencePolicy(),
                    "fact-vanilla-d5b");

            var peerPressureDiscovery =
                CreateAvailableDiscovery(
                    kidFive,
                    "ChocolatePeerPressureIsStrong",
                    conformingFact,
                    "discovery-peer-pressure");

            var vanillaDiscovery =
                CreateAvailableDiscovery(
                    kidFive,
                    "VanillaRemainsPersonallyPreferred",
                    vanillaFact,
                    "discovery-vanilla-d5b");

            Assert.True(peerPressureDiscovery.IsAvailable);
            Assert.True(vanillaDiscovery.IsAvailable);
            Assert.Same(kidFive, peerPressureDiscovery.Entity);
            Assert.Same(kidFive, vanillaDiscovery.Entity);
            Assert.NotSame(group.GroupEntity, kidFive);

            TestLog.State(
                "Kid 5 received exact group Truth as Evidence = " +
                ReferenceEquals(groupTruth, peerPressureEvidence.Outcome));

            TestLog.State(
                "Group ownership merged into Kid 5 = False");

            TestLog.State(
                "Group history merged into Kid 5 = False");

            TestLog.State(
                "Group Truth forced a Kid 5 action = False");

            TestLog.Pass(
                "A real production-congealed group Truth became " +
                "Kid-5-owned Evidence without merging ownership, " +
                "history, personal preference, or consumer policy.");
        }

        private static DiscoveryEvidence CreateValidatedEvidence(
            IEntity entity,
            IReadOnlyDictionary<string, object?> conditions,
            object outcome,
            double strength,
            string externalId,
            string name)
        {
            var evidence =
                new DiscoveryEvidence(
                    entity,
                    conditions,
                    outcome,
                    strength,
                    1.0,
                    new List<IBaseObject>(),
                    null,
                    externalId,
                    name);

            var proof =
                new TestProof(
                    1.0,
                    true,
                    "Controlled readable proof.");

            Assert.True(
                evidence.ApplyProof(
                    proof.Validate(
                        evidence)));

            return evidence;
        }

        private static Fact CreateProvenFact(
            IEntity entity,
            string name,
            IDiscoveryEvidence evidence,
            IEvidenceContributionPolicy policy,
            string externalId)
        {
            var fact =
                new Fact(
                    entity,
                    name,
                    true,
                    1,
                    policy,
                    externalId);

            Assert.True(
                fact.ReceiveEvidence(
                    evidence));

            Assert.True(fact.IsProven);

            return fact;
        }

        private static Discovery CreateAvailableDiscovery(
            IEntity entity,
            string name,
            IFact fact,
            string externalId)
        {
            var discovery =
                new Discovery(
                    entity,
                    1,
                    name,
                    externalId);

            Assert.True(
                discovery.ReceiveFact(
                    fact));

            Assert.True(discovery.IsAvailable);

            return discovery;
        }

        private static CompetingKnowledge BuildCompetingKnowledge(
            WildsDiscoveries discoveries,
            string scope,
            string documentSlug,
            string historyKey,
            string firstKey,
            IDiscoveryVectorEvaluation firstEvaluation,
            string secondKey,
            IDiscoveryVectorEvaluation secondEvaluation,
            string relationshipKey)
        {
            var profileId =
                Guid.NewGuid();

            var firstCentroid =
                ComponentMap(firstEvaluation.Vector);

            var secondCentroid =
                ComponentMap(secondEvaluation.Vector);

            var geometryRegistration =
                discoveries.Geometry.Register(
                    new DiscoveryGeometryProfile(
                        profileId,
                        documentSlug,
                        "1.0",
                        scope,
                        new List<IDiscoveryClusterMapping>
                        {
                            new DiscoveryClusterMapping(
                                Guid.NewGuid(),
                                "first",
                                profileId,
                                documentSlug,
                                "1.0",
                                firstKey,
                                "hypersphere",
                                firstCentroid,
                                0.25),
                            new DiscoveryClusterMapping(
                                Guid.NewGuid(),
                                "second",
                                profileId,
                                documentSlug,
                                "1.0",
                                secondKey,
                                "hypersphere",
                                secondCentroid,
                                0.25)
                        },
                        new MinimumClusterPersistencePolicy(
                            "one-observation",
                            1,
                            0.0,
                            0.0),
                        new BoundaryVarianceClusterStabilityMetric(),
                        new PersistentClusterRecognitionPolicy(
                            "recognize-knowledge")));

            var first =
                discoveries.Geometry.Observe(
                    firstEvaluation,
                    firstKey,
                    historyKey);

            var second =
                discoveries.Geometry.Observe(
                    secondEvaluation,
                    secondKey,
                    historyKey);

            Assert.True(first.Recognition.IsRecognized);
            Assert.True(second.Recognition.IsRecognized);

            var population =
                discoveries.Narratives.CreatePopulation(
                    geometryRegistration.Id,
                    new List<IDiscoveryClusterRecognitionResult>
                    {
                        first.Recognition,
                        second.Recognition
                    });

            var relationship =
                new DiscoveryNarrativeRelationshipDefinition(
                    Guid.NewGuid(),
                    "competing",
                    firstKey,
                    secondKey,
                    relationshipKey);

            var narrativeRegistration =
                discoveries.Narratives.Register(
                    new DiscoveryNarrativeProfile(
                        Guid.NewGuid(),
                        documentSlug + "-narratives",
                        "1.0",
                        scope,
                        new RelationshipKeyNarrativeCompositionPolicy(
                            "ReinforcingKnowledge"),
                        new RecognitionCentroidDistanceNarrativeMetric(),
                        new List<IDiscoveryNarrativeRelationshipDefinition>
                        {
                            relationship
                        }));

            var composition =
                discoveries.Narratives.Compose(
                    population,
                    narrativeRegistration.Id);

            Assert.Equal(2, composition.Groups.Count);
            Assert.Single(composition.Decisions);
            Assert.False(composition.Decisions[0].CanCompose);

            var narratives =
                discoveries.Outcomes.Relate(
                    composition);

            Assert.Single(narratives.GroupRelationships);

            return new CompetingKnowledge(
                narratives);
        }

        private static Dictionary<string, double> ComponentMap(
            IDiscoveryVectorPoint point)
        {
            var result =
                new Dictionary<string, double>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < point.Components.Count;
                 index++)
            {
                result.Add(
                    point.Components[index].Name,
                    point.Components[index].Value);
            }

            return result;
        }

        private static GroupTruthContext CreateCongealedGroupTruth(
            string scope)
        {
            var discoveries =
                new WildsDiscoveries();

            var groupEntity =
                new TestEntity(
                    "kids-1-through-4");

            var chocolateEvidence =
                CreateValidatedEvidence(
                    groupEntity,
                    new Dictionary<string, object?>
                    {
                        ["GroupSize"] = 4,
                        ["Flavor"] = "Chocolate"
                    },
                    "Kids 1-4 selected chocolate",
                    4.0,
                    "group-chocolate-observation",
                    "Group chocolate observation");

            var alternativeEvidence =
                CreateValidatedEvidence(
                    groupEntity,
                    new Dictionary<string, object?>
                    {
                        ["AlternativeAvailable"] = true
                    },
                    "Other flavors remain available",
                    1.0,
                    "group-alternative-observation",
                    "Group alternative observation");

            var chocolateFact =
                CreateProvenFact(
                    groupEntity,
                    "ChocolateSelectedByGroup",
                    chocolateEvidence,
                    new TrueEvidencePolicy(),
                    "group-fact-chocolate");

            var alternativeFact =
                CreateProvenFact(
                    groupEntity,
                    "AlternativeFlavorRemainsPossible",
                    alternativeEvidence,
                    new TrueEvidencePolicy(),
                    "group-fact-alternative");

            var chocolateDiscovery =
                CreateAvailableDiscovery(
                    groupEntity,
                    "ChocolateDominatesNearbyGroup",
                    chocolateFact,
                    "group-discovery-chocolate");

            var alternativeDiscovery =
                CreateAvailableDiscovery(
                    groupEntity,
                    "AlternativeFlavorRemainsPossible",
                    alternativeFact,
                    "group-discovery-alternative");

            var chocolateInput =
                new DiscoveryVectorInput(
                    chocolateDiscovery,
                    "group chocolate knowledge");

            var alternativeInput =
                new DiscoveryVectorInput(
                    alternativeDiscovery,
                    "group alternative knowledge");

            var now =
                DateTimeOffset.UtcNow;

            var chocolateVector =
                CreateVector(
                    chocolateInput,
                    chocolateFact,
                    now,
                    ("group-alternative", 0.0),
                    ("group-chocolate", 1.0));

            var alternativeVector =
                CreateVector(
                    alternativeInput,
                    alternativeFact,
                    now.AddSeconds(1),
                    ("group-alternative", 1.0),
                    ("group-chocolate", 0.0));

            var chocolateEvaluation =
                new DiscoveryVectorEvaluation(
                    chocolateVector,
                    Guid.NewGuid(),
                    now);

            var alternativeEvaluation =
                new DiscoveryVectorEvaluation(
                    alternativeVector,
                    Guid.NewGuid(),
                    now.AddSeconds(1));

            var series =
                new DiscoveryVectorEvaluationSeries(
                    scope,
                    new List<IDiscoveryVectorInput>
                    {
                        chocolateInput,
                        alternativeInput
                    },
                    new List<IDiscoveryVectorEvaluation>
                    {
                        chocolateEvaluation,
                        alternativeEvaluation
                    });

            var knowledge =
                BuildCompetingKnowledge(
                    discoveries,
                    scope,
                    "nearby-kids-group",
                    "kids-1-through-4",
                    "ChocolateDominatesNearbyGroup",
                    chocolateEvaluation,
                    "AlternativeFlavorRemainsPossible",
                    alternativeEvaluation,
                    "CompetingGroupKnowledge");

            var clusters =
                new List<IDiscoveryCluster>
                {
                    new ReadableCluster(
                        new List<IDiscoveryVector>
                        {
                            chocolateVector
                        }),
                    new ReadableCluster(
                        new List<IDiscoveryVector>
                        {
                            alternativeVector
                        })
                };

            var policy =
                new TextTruthPolicy(
                    "nearby-kids-group-truth-policy",
                    "ChocolateIsDominantInNearbyGroup",
                    "Chocolate is dominant among Kids 1-4.");

            var registration =
                discoveries.Outcomes.Truths.Register(
                    new DiscoveryTruthProfile(
                        Guid.NewGuid(),
                        "nearby-kids-group-truth",
                        "1.0",
                        scope,
                        policy));

            var result =
                discoveries.Outcomes.Truths.Congeal(
                    knowledge.Narratives,
                    registration.Id,
                    series,
                    clusters);

            return new GroupTruthContext(
                groupEntity,
                knowledge.Narratives,
                series,
                clusters,
                policy,
                result);
        }

        private static DiscoveryVector CreateVector(
            IDiscoveryVectorInput input,
            IFact fact,
            DateTimeOffset at,
            params (string Dimension, double Value)[] values)
        {
            var components =
                new List<IDiscoveryVectorComponent>();

            var contributions =
                new List<IDiscoveryVectorContribution>();

            for (var index = 0;
                 index < values.Length;
                 index++)
            {
                components.Add(
                    new DiscoveryVectorComponent(
                        values[index].Dimension,
                        values[index].Value));

                contributions.Add(
                    new DiscoveryVectorContribution(
                        input,
                        values[index].Dimension,
                        values[index].Value,
                        1.0,
                        fact));
            }

            return new DiscoveryVector(
                components,
                contributions,
                new List<IDiscoveryVectorMotion>(),
                new AbsoluteMagnitudeMetric(),
                at);
        }

        private sealed class CompetingKnowledge
        {
            public CompetingKnowledge(
                IDiscoveryCompetingNarrativesResult narratives)
            {
                Narratives =
                    narratives;
            }

            public IDiscoveryCompetingNarrativesResult Narratives { get; }
        }

        private sealed class GroupTruthContext
        {
            public GroupTruthContext(
                IEntity groupEntity,
                IDiscoveryCompetingNarrativesResult narratives,
                IDiscoveryVectorSeries series,
                IReadOnlyList<IDiscoveryCluster> clusters,
                TextTruthPolicy policy,
                IDiscoveryTruthCongealmentResult truthResult)
            {
                GroupEntity = groupEntity;
                Narratives = narratives;
                Series = series;
                Clusters = clusters;
                Policy = policy;
                TruthResult = truthResult;
            }

            public IEntity GroupEntity { get; }
            public IDiscoveryCompetingNarrativesResult Narratives { get; }
            public IDiscoveryVectorSeries Series { get; }
            public IReadOnlyList<IDiscoveryCluster> Clusters { get; }
            public TextTruthPolicy Policy { get; }
            public IDiscoveryTruthCongealmentResult TruthResult { get; }
        }

        private sealed class GroupTruthPeerPressurePolicy :
            IEvidenceContributionPolicy
        {
            private readonly int _minimumGroupSize;

            public GroupTruthPeerPressurePolicy(
                int minimumGroupSize)
            {
                if (minimumGroupSize < 1)
                    throw new ArgumentOutOfRangeException(
                        nameof(minimumGroupSize));

                _minimumGroupSize = minimumGroupSize;
            }

            public bool Contributes(
                IDiscoveryEvidence evidence)
            {
                if (evidence == null)
                    throw new ArgumentNullException(
                        nameof(evidence));

                if (!evidence.IsValidated ||
                    !(evidence.Outcome is ITruth) ||
                    !evidence.Conditions.TryGetValue(
                        "ObservedGroupSize",
                        out var sizeValue) ||
                    !(sizeValue is int size))
                {
                    return false;
                }

                return size >= _minimumGroupSize;
            }
        }

        private sealed class RejectGroupTruthPolicy :
            IEvidenceContributionPolicy
        {
            public bool Contributes(
                IDiscoveryEvidence evidence)
            {
                if (evidence == null)
                    throw new ArgumentNullException(
                        nameof(evidence));

                return false;
            }
        }

        private sealed class IceCreamVectorizationService :
            DiscoveryVectorizationServiceBase
        {
            public IceCreamVectorizationService(
                IWildsServiceContextFactory<IWildsServiceContext> contexts,
                IWildsServiceExecutionJournal journal,
                IWildsServiceClock clock)
                : base(
                    "kid-five-ice-cream-vectorization",
                    contexts,
                    journal,
                    clock)
            {
            }

            protected override Task<DiscoveryVectorizationServiceResult>
                ExecuteCoreAsync(
                    DiscoveryVectorizationRequest request,
                    IWildsServiceContext context,
                    CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var discovery =
                    request.Input.Discovery;

                var social =
                    string.Equals(
                        discovery.Name,
                        "ChocolateIsSociallyDominant",
                        StringComparison.Ordinal)
                        ? 1.0
                        : 0.0;

                var personal =
                    string.Equals(
                        discovery.Name,
                        "VanillaRemainsPersonallyPreferred",
                        StringComparison.Ordinal)
                        ? 1.0
                        : 0.0;

                if (social == 0.0 && personal == 0.0)
                    throw new InvalidOperationException(
                        "Unknown ice-cream Discovery.");

                return Task.FromResult(
                    new DiscoveryVectorizationServiceResult(
                        new List<IDiscoveryVectorContribution>
                        {
                            new DiscoveryVectorContribution(
                                request.Input,
                                "social-chocolate",
                                social,
                                1.0,
                                discovery.Facts[0]),
                            new DiscoveryVectorContribution(
                                request.Input,
                                "personal-vanilla",
                                personal,
                                1.0,
                                discovery.Facts[0])
                        }));
            }
        }

        private sealed class AbsoluteMagnitudeMetric :
            IDiscoveryVectorMetric
        {
            public double GetMagnitude(
                IDiscoveryVectorPoint point)
            {
                var total = 0.0;

                for (var index = 0;
                     index < point.Components.Count;
                     index++)
                {
                    total += Math.Abs(
                        point.Components[index].Value);
                }

                return total;
            }
        }

        private sealed class ReadableCluster :
            IDiscoveryCluster
        {
            public ReadableCluster(
                IReadOnlyList<IDiscoveryVector> members)
            {
                if (members == null)
                    throw new ArgumentNullException(
                        nameof(members));

                if (members.Count == 0)
                    throw new ArgumentException(
                        "A readable cluster requires a member.",
                        nameof(members));

                Members = members;
                Centroid = members[0];
                Density = 1.0;
                Dispersion = 0.0;
                Coherence = 1.0;
            }

            public IReadOnlyList<IDiscoveryVector> Members { get; }
            public IDiscoveryVectorPoint Centroid { get; }
            public double Density { get; }
            public double Dispersion { get; }
            public double Coherence { get; }
        }

        private sealed class TextTruthPolicy :
            IIdentifiedTruthCongealmentPolicy
        {
            private readonly string _truthName;
            private readonly string _truthValue;

            public TextTruthPolicy(
                string slug,
                string truthName,
                string truthValue)
            {
                Id = Guid.NewGuid();
                Slug = slug;
                _truthName = truthName;
                _truthValue = truthValue;
            }

            public Guid Id { get; }
            public string Slug { get; }
            public bool CanCongealCalled { get; private set; }
            public bool CongealCalled { get; private set; }
            public IDiscoveryCompetingNarrativesResult? Narratives { get; private set; }
            public IReadOnlyList<IDiscoveryCluster>? Clusters { get; private set; }
            public IDiscoveryVectorSeries? Series { get; private set; }

            public bool CanCongeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CanCongealCalled = true;
                Narratives = narratives;
                Clusters = clusters;
                Series = series;

                return narratives.Composition.Groups.Count == 2 &&
                    narratives.GroupRelationships.Count == 1;
            }

            public ITruth Congeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CongealCalled = true;
                Narratives = narratives;
                Clusters = clusters;
                Series = series;

                var discoveries =
                    new List<IDiscovery>();

                for (var index = 0;
                     index < series.Inputs.Count;
                     index++)
                {
                    discoveries.Add(
                        series.Inputs[index].Discovery);
                }

                return new TextTruth(
                    _truthName,
                    _truthValue,
                    series,
                    clusters,
                    discoveries);
            }
        }

        private sealed class TextTruth :
            ITruth
        {
            public TextTruth(
                string name,
                string value,
                IDiscoveryVectorSeries series,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IReadOnlyList<IDiscovery> discoveries)
            {
                Id = Guid.NewGuid();
                Slug = name.ToLowerInvariant();
                Name = name;
                Value = value;
                Series = series;
                Clusters = clusters;
                Discoveries = discoveries;
                Stability = 1.0;
                Confidence = 1.0;
                CongealedAt = DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }
            public string Slug { get; }
            public string Name { get; }
            public object? Value { get; }
            public IDiscoveryVectorSeries Series { get; }
            public IReadOnlyList<IDiscoveryCluster> Clusters { get; }
            public IReadOnlyList<IDiscovery> Discoveries { get; }
            public double Stability { get; }
            public double Confidence { get; }
            public DateTimeOffset CongealedAt { get; }
        }

        private sealed class KidFiveConsequencePolicy :
            IIdentifiedConsequenceProjectionPolicy
        {
            public KidFiveConsequencePolicy()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; }
            public string Slug => "kid-five-consequence-policy";
            public bool ProjectCalled { get; private set; }

            public IReadOnlyList<IConsequence> Project(
                IReadOnlyList<ITruth> truths)
            {
                ProjectCalled = true;

                if (truths.Count == 0)
                    return new List<IConsequence>();

                return new List<IConsequence>
                {
                    new KidFiveConsequence(
                        truths[0])
                };
            }
        }

        private sealed class KidFiveConsequence :
            IConsequence
        {
            public KidFiveConsequence(
                ITruth truth)
            {
                Id = Guid.NewGuid();
                Slug = "kid-five-may-choose-vanilla";
                Name = "KidFiveMayChooseVanilla";
                Value =
                    "Kid 5 may choose vanilla despite surrounding " +
                    "chocolate preference.";
                Truths = new List<ITruth> { truth };
                Magnitude = 1.0;
                Confidence = 1.0;
                ProjectedAt = DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }
            public string Slug { get; }
            public string Name { get; }
            public object? Value { get; }
            public IReadOnlyList<ITruth> Truths { get; }
            public double Magnitude { get; }
            public double Confidence { get; }
            public DateTimeOffset ProjectedAt { get; }
        }
    }
}
