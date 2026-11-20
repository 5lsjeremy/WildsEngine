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
    /// D5 readable end-to-end certification.
    ///
    /// Kid 5 observes four nearby chocolate choices while retaining
    /// a separate personal preference for vanilla.
    ///
    /// This test follows the production Discoveries path:
    ///
    /// Consumer Reality
    ///     -> Evidence
    ///     -> Proof
    ///     -> Fact
    ///     -> Discovery
    ///     -> Vectorization
    ///     -> Geometry
    ///     -> Persistence / Recognition
    ///     -> Competing Narratives
    ///     -> Consumer-defined Truth
    ///     -> Consumer-defined Consequence
    ///     -> STOP
    ///
    /// Discoveries carries and organizes knowledge. It never chooses
    /// an ice-cream flavor or executes behavior for Kid 5.
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

            // ---------------------------------------------------------
            // CONSUMER REALITY -> KID-5-OWNED EVIDENCE
            // ---------------------------------------------------------
            //
            // The other children do not donate their Evidence,
            // Discoveries, or geometry histories to Kid 5.
            //
            // Kid 5 owns Evidence describing what Kid 5 observed
            // about the surrounding world.
            var chocolateEvidence =
                new DiscoveryEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["NearbyKidCount"] = 4,
                        ["ObservedFlavor"] = "Chocolate",
                        ["Location"] = "IceCreamStand"
                    },
                    outcome:
                        "Four nearby kids selected chocolate",
                    strength:
                        4.0,
                    normalizedStrength:
                        1.0,
                    neighbors:
                        new List<IBaseObject>(),
                    source:
                        null,
                    externalId:
                        "kid-5-observation-chocolate-4",
                    name:
                        "Nearby chocolate choices");

            var vanillaEvidence =
                new DiscoveryEvidence(
                    kidFive,
                    new Dictionary<string, object?>
                    {
                        ["Subject"] = "Kid 5",
                        ["PreferredFlavor"] = "Vanilla",
                        ["PreferenceSource"] = "Personal"
                    },
                    outcome:
                        "Kid 5 personally prefers vanilla",
                    strength:
                        1.0,
                    normalizedStrength:
                        1.0,
                    neighbors:
                        new List<IBaseObject>(),
                    source:
                        null,
                    externalId:
                        "kid-5-personal-vanilla",
                    name:
                        "Kid 5 vanilla preference");

            Assert.Same(
                kidFive,
                chocolateEvidence.Entity);

            Assert.Same(
                kidFive,
                vanillaEvidence.Entity);

            TestLog.State(
                "Nearby kids choosing chocolate = " +
                chocolateEvidence.Conditions["NearbyKidCount"]);

            TestLog.State(
                "Chocolate observation belongs to Kid 5 = " +
                ReferenceEquals(
                    kidFive,
                    chocolateEvidence.Entity));

            TestLog.State(
                "Vanilla preference Evidence belongs to Kid 5 = " +
                ReferenceEquals(
                    kidFive,
                    vanillaEvidence.Entity));

            // ---------------------------------------------------------
            // EVIDENCE -> PROOF
            // ---------------------------------------------------------
            //
            // Proof remains consumer supplied. Discoveries does not
            // decide whether either observation is true or believable.
            var chocolateProof =
                new TestProof(
                    1.0,
                    true,
                    "Kid 5 directly observed four chocolate choices.");

            var vanillaProof =
                new TestProof(
                    1.0,
                    true,
                    "Kid 5 directly reports a personal vanilla preference.");

            Assert.True(
                chocolateEvidence.ApplyProof(
                    chocolateProof.Validate(
                        chocolateEvidence)));

            Assert.True(
                vanillaEvidence.ApplyProof(
                    vanillaProof.Validate(
                        vanillaEvidence)));

            Assert.True(
                chocolateEvidence.IsValidated);

            Assert.True(
                vanillaEvidence.IsValidated);

            TestLog.State(
                "Chocolate observation validated = " +
                chocolateEvidence.IsValidated);

            TestLog.State(
                "Vanilla preference validated = " +
                vanillaEvidence.IsValidated);

            // ---------------------------------------------------------
            // PROOF -> FACT
            // ---------------------------------------------------------
            //
            // These Facts express different kinds of knowledge:
            //
            // 1. Chocolate is popular nearby.
            // 2. Kid 5 personally prefers vanilla.
            //
            // They can both be true.
            var chocolatePopularityFact =
                new Fact(
                    kidFive,
                    "ChocolateIsPopularNearby",
                    true,
                    requiredEvidenceCount:
                        1,
                    contributionPolicy:
                        new TrueEvidencePolicy(),
                    externalId:
                        "fact-chocolate-popular");

            var vanillaPreferenceFact =
                new Fact(
                    kidFive,
                    "VanillaRemainsPersonallyPreferred",
                    true,
                    requiredEvidenceCount:
                        1,
                    contributionPolicy:
                        new TrueEvidencePolicy(),
                    externalId:
                        "fact-vanilla-preferred");

            Assert.True(
                chocolatePopularityFact.ReceiveEvidence(
                    chocolateEvidence));

            Assert.True(
                vanillaPreferenceFact.ReceiveEvidence(
                    vanillaEvidence));

            Assert.True(
                chocolatePopularityFact.IsProven);

            Assert.True(
                vanillaPreferenceFact.IsProven);

            Assert.Same(
                chocolateEvidence,
                chocolatePopularityFact.Contributors[0]);

            Assert.Same(
                vanillaEvidence,
                vanillaPreferenceFact.Contributors[0]);

            TestLog.State(
                "Chocolate-popularity Fact proven = " +
                chocolatePopularityFact.IsProven);

            TestLog.State(
                "Vanilla-preference Fact proven = " +
                vanillaPreferenceFact.IsProven);

            // ---------------------------------------------------------
            // FACT -> DISCOVERY
            // ---------------------------------------------------------
            var chocolateDiscovery =
                new Discovery(
                    kidFive,
                    requiredFactCount:
                        1,
                    name:
                        "ChocolateIsSociallyDominant",
                    externalId:
                        "discovery-social-chocolate");

            var vanillaDiscovery =
                new Discovery(
                    kidFive,
                    requiredFactCount:
                        1,
                    name:
                        "VanillaRemainsPersonallyPreferred",
                    externalId:
                        "discovery-personal-vanilla");

            Assert.True(
                chocolateDiscovery.ReceiveFact(
                    chocolatePopularityFact));

            Assert.True(
                vanillaDiscovery.ReceiveFact(
                    vanillaPreferenceFact));

            Assert.True(
                chocolateDiscovery.IsAvailable);

            Assert.True(
                vanillaDiscovery.IsAvailable);

            Assert.Same(
                kidFive,
                chocolateDiscovery.Entity);

            Assert.Same(
                kidFive,
                vanillaDiscovery.Entity);

            TestLog.State(
                "Discovery A = " +
                chocolateDiscovery.Name);

            TestLog.State(
                "Discovery B = " +
                vanillaDiscovery.Name);

            TestLog.State(
                "Chocolate Discovery available = " +
                chocolateDiscovery.IsAvailable);

            TestLog.State(
                "Vanilla Discovery available = " +
                vanillaDiscovery.IsAvailable);

            // ---------------------------------------------------------
            // DISCOVERY -> PRODUCTION VECTORIZATION SERVICE
            // ---------------------------------------------------------
            var clock =
                new SystemWildsServiceClock();

            var journal =
                new WildsServiceExecutionJournal();

            var contexts =
                new WildsServiceContextFactory(
                    clock);

            var vectorizationService =
                new IceCreamVectorizationService(
                    contexts,
                    journal,
                    clock);

            var vectorizer =
                new DiscoveryVectorizerFactory()
                    .Create(
                        vectorizationService,
                        new AbsoluteMagnitudeMetric());

            var chocolateInput =
                new DiscoveryVectorInput(
                    chocolateDiscovery,
                    provenance:
                        "Kid 5 social observation");

            var vanillaInput =
                new DiscoveryVectorInput(
                    vanillaDiscovery,
                    provenance:
                        "Kid 5 personal preference");

            var chocolateVectorization =
                await vectorizer.VectorizeAsync(
                    chocolateInput,
                    provenance:
                        "D5 chocolate vectorization",
                    pulseId:
                        Guid.NewGuid());

            var vanillaVectorization =
                await vectorizer.VectorizeAsync(
                    vanillaInput,
                    provenance:
                        "D5 vanilla vectorization",
                    pulseId:
                        Guid.NewGuid());

            Assert.NotEqual(
                Guid.Empty,
                chocolateVectorization.ServiceExecutionId);

            Assert.NotEqual(
                Guid.Empty,
                vanillaVectorization.ServiceExecutionId);

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

            Assert.Same(
                chocolateInput,
                chocolateVectorization
                    .Vector
                    .Contributions[0]
                    .Input);

            Assert.Same(
                vanillaInput,
                vanillaVectorization
                    .Vector
                    .Contributions[0]
                    .Input);

            TestLog.State(
                "Chocolate vectorization execution = " +
                chocolateVectorization.ServiceExecutionId);

            TestLog.State(
                "Vanilla vectorization execution = " +
                vanillaVectorization.ServiceExecutionId);

            TestLog.State(
                "Chocolate vector social-chocolate = " +
                GetComponent(
                    chocolateVectorization.Vector,
                    "social-chocolate"));

            TestLog.State(
                "Chocolate vector personal-vanilla = " +
                GetComponent(
                    chocolateVectorization.Vector,
                    "personal-vanilla"));

            TestLog.State(
                "Vanilla vector social-chocolate = " +
                GetComponent(
                    vanillaVectorization.Vector,
                    "social-chocolate"));

            TestLog.State(
                "Vanilla vector personal-vanilla = " +
                GetComponent(
                    vanillaVectorization.Vector,
                    "personal-vanilla"));

            // ---------------------------------------------------------
            // VECTOR -> EVALUATION / AUTHORITATIVE SERIES
            // ---------------------------------------------------------
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

            Assert.Equal(
                2,
                series.Inputs.Count);

            Assert.Same(
                chocolateDiscovery,
                series.Inputs[0].Discovery);

            Assert.Same(
                vanillaDiscovery,
                series.Inputs[1].Discovery);

            Assert.Same(
                chocolateInput,
                series.Inputs[0]);

            Assert.Same(
                vanillaInput,
                series.Inputs[1]);

            // ---------------------------------------------------------
            // EVALUATION -> GEOMETRY / RECOGNITION
            // ---------------------------------------------------------
            var discoveries =
                new WildsDiscoveries();

            var geometryProfileId =
                Guid.NewGuid();

            const string chocolateKey =
                "ChocolateIsSociallyDominant";

            const string vanillaKey =
                "VanillaRemainsPersonallyPreferred";

            var mappings =
                new List<IDiscoveryClusterMapping>
                {
                    new DiscoveryClusterMapping(
                        Guid.NewGuid(),
                        "social-chocolate",
                        geometryProfileId,
                        "kid-five-ice-cream",
                        "1.0",
                        chocolateKey,
                        "hypersphere",
                        new Dictionary<string, double>
                        {
                            ["personal-vanilla"] = 0.0,
                            ["social-chocolate"] = 1.0
                        },
                        radius:
                            0.25),

                    new DiscoveryClusterMapping(
                        Guid.NewGuid(),
                        "personal-vanilla",
                        geometryProfileId,
                        "kid-five-ice-cream",
                        "1.0",
                        vanillaKey,
                        "hypersphere",
                        new Dictionary<string, double>
                        {
                            ["personal-vanilla"] = 1.0,
                            ["social-chocolate"] = 0.0
                        },
                        radius:
                            0.25)
                };

            var geometryRegistration =
                discoveries.Geometry.Register(
                    new DiscoveryGeometryProfile(
                        geometryProfileId,
                        "kid-five-ice-cream",
                        "1.0",
                        scope,
                        mappings,
                        new MinimumClusterPersistencePolicy(
                            "one-authoritative-observation",
                            minimumConsecutiveInside:
                                1,
                            minimumOccupancyRatio:
                                0.0,
                            minimumStability:
                                0.0),
                        new BoundaryVarianceClusterStabilityMetric(),
                        new PersistentClusterRecognitionPolicy(
                            "recognize-persistent-kid-five-knowledge")));

            const string kidFiveHistory =
                "kid-5";

            var chocolateObservation =
                discoveries.Geometry.Observe(
                    chocolateEvaluation,
                    chocolateKey,
                    kidFiveHistory);

            var vanillaObservation =
                discoveries.Geometry.Observe(
                    vanillaEvaluation,
                    vanillaKey,
                    kidFiveHistory);

            Assert.True(
                chocolateObservation.Persistence.IsPersistent);

            Assert.True(
                vanillaObservation.Persistence.IsPersistent);

            Assert.True(
                chocolateObservation.Recognition.IsRecognized);

            Assert.True(
                vanillaObservation.Recognition.IsRecognized);

            Assert.Equal(
                1,
                chocolateObservation.Persistence.ObservationCount);

            Assert.Equal(
                1,
                vanillaObservation.Persistence.ObservationCount);

            TestLog.State(
                "Geometry history = " +
                kidFiveHistory);

            TestLog.State(
                "Social chocolate recognized = " +
                chocolateObservation.Recognition.IsRecognized);

            TestLog.State(
                "Personal vanilla recognized = " +
                vanillaObservation.Recognition.IsRecognized);

            // ---------------------------------------------------------
            // RECOGNITION -> POPULATION -> COMPETING NARRATIVES
            // ---------------------------------------------------------
            var population =
                discoveries.Narratives.CreatePopulation(
                    geometryRegistration.Id,
                    new List<IDiscoveryClusterRecognitionResult>
                    {
                        chocolateObservation.Recognition,
                        vanillaObservation.Recognition
                    });

            var competingDefinition =
                new DiscoveryNarrativeRelationshipDefinition(
                    Guid.NewGuid(),
                    "social-versus-personal",
                    chocolateKey,
                    vanillaKey,
                    "CompetingKnowledge");

            var narrativeProfile =
                new DiscoveryNarrativeProfile(
                    Guid.NewGuid(),
                    "kid-five-competing-narratives",
                    "1.0",
                    scope,
                    new RelationshipKeyNarrativeCompositionPolicy(
                        "ReinforcingKnowledge"),
                    new RecognitionCentroidDistanceNarrativeMetric(),
                    new List<IDiscoveryNarrativeRelationshipDefinition>
                    {
                        competingDefinition
                    });

            var narrativeRegistration =
                discoveries.Narratives.Register(
                    narrativeProfile);

            var composition =
                discoveries.Narratives.Compose(
                    population,
                    narrativeRegistration.Id);

            // The supplied relationship is intentionally not accepted
            // as reinforcing composition, so the two narratives remain
            // separate and become a cross-group competing relationship.
            Assert.Equal(
                2,
                composition.Groups.Count);

            Assert.Single(
                composition.Decisions);

            Assert.False(
                composition.Decisions[0].CanCompose);

            Assert.Same(
                competingDefinition,
                composition.Decisions[0].Relationship);

            var competingNarratives =
                discoveries.Outcomes.Relate(
                    composition);

            Assert.Single(
                competingNarratives.GroupRelationships);

            Assert.Single(
                competingNarratives
                    .GroupRelationships[0]
                    .Definitions);

            Assert.Same(
                competingDefinition,
                competingNarratives
                    .GroupRelationships[0]
                    .Definitions[0]);

            TestLog.State(
                "Narrative A = " +
                chocolateKey);

            TestLog.State(
                "Narrative B = " +
                vanillaKey);

            TestLog.State(
                "Narrative relationship = " +
                competingDefinition.RelationshipKey);

            // ---------------------------------------------------------
            // NARRATIVES + CLUSTERS + SERIES -> CONSUMER TRUTH
            // ---------------------------------------------------------
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
                new KidFiveTruthPolicy();

            var truthRegistration =
                discoveries
                    .Outcomes
                    .Truths
                    .Register(
                        new DiscoveryTruthProfile(
                            Guid.NewGuid(),
                            "kid-five-truth",
                            "1.0",
                            scope,
                            truthPolicy));

            var truthResult =
                discoveries
                    .Outcomes
                    .Truths
                    .Congeal(
                        competingNarratives,
                        truthRegistration.Id,
                        series,
                        clusters);

            Assert.True(
                truthResult.Congealed);

            Assert.Single(
                truthResult.Truths);

            Assert.True(
                truthPolicy.CanCongealCalled);

            Assert.True(
                truthPolicy.CongealCalled);

            Assert.Same(
                competingNarratives,
                truthPolicy.Narratives);

            Assert.Same(
                series,
                truthPolicy.Series);

            Assert.Same(
                clusters,
                truthPolicy.Clusters);

            var truth =
                truthResult.Truths[0];

            Assert.Equal(
                "Chocolate is socially dominant while " +
                "Kid 5 still personally prefers vanilla.",
                truth.Value);

            Assert.Same(
                series,
                truth.Series);

            Assert.Equal(
                2,
                truth.Discoveries.Count);

            Assert.Same(
                chocolateDiscovery,
                truth.Discoveries[0]);

            Assert.Same(
                vanillaDiscovery,
                truth.Discoveries[1]);

            TestLog.State(
                "Truth = " +
                truth.Value);

            // ---------------------------------------------------------
            // TRUTH -> CONSUMER CONSEQUENCE
            // ---------------------------------------------------------
            var consequencePolicy =
                new KidFiveConsequencePolicy();

            var consequenceRegistration =
                discoveries
                    .Outcomes
                    .Consequences
                    .Register(
                        new DiscoveryConsequenceProfile(
                            Guid.NewGuid(),
                            "kid-five-consequence",
                            "1.0",
                            scope,
                            consequencePolicy));

            var consequenceResult =
                discoveries
                    .Outcomes
                    .Consequences
                    .Project(
                        truthResult,
                        consequenceRegistration.Id);

            Assert.True(
                consequencePolicy.ProjectCalled);

            Assert.Single(
                consequenceResult.Consequences);

            var consequence =
                consequenceResult.Consequences[0];

            Assert.Equal(
                "Kid 5 may choose vanilla despite surrounding " +
                "chocolate preference.",
                consequence.Value);

            Assert.Single(
                consequence.Truths);

            Assert.Same(
                truth,
                consequence.Truths[0]);

            TestLog.State(
                "Consequence = " +
                consequence.Value);

            TestLog.Pass(
                "Discoveries carried Kid 5 from observation through " +
                "competing knowledge to projected consequence without " +
                "choosing or executing an action for him.");
        }

        private static double GetComponent(
            IDiscoveryVectorPoint vector,
            string dimension)
        {
            for (var index = 0;
                 index < vector.Components.Count;
                 index++)
            {
                if (string.Equals(
                        vector.Components[index].Name,
                        dimension,
                        StringComparison.Ordinal))
                {
                    return vector.Components[index].Value;
                }
            }

            throw new KeyNotFoundException(
                "Vector component was not found: " +
                dimension);
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

                if (string.Equals(
                        discovery.Name,
                        "ChocolateIsSociallyDominant",
                        StringComparison.Ordinal))
                {
                    return Task.FromResult(
                        new DiscoveryVectorizationServiceResult(
                            new List<IDiscoveryVectorContribution>
                            {
                                new DiscoveryVectorContribution(
                                    request.Input,
                                    "social-chocolate",
                                    1.0,
                                    1.0,
                                    discovery.Facts[0]),

                                new DiscoveryVectorContribution(
                                    request.Input,
                                    "personal-vanilla",
                                    0.0,
                                    1.0,
                                    discovery.Facts[0])
                            }));
                }

                if (string.Equals(
                        discovery.Name,
                        "VanillaRemainsPersonallyPreferred",
                        StringComparison.Ordinal))
                {
                    return Task.FromResult(
                        new DiscoveryVectorizationServiceResult(
                            new List<IDiscoveryVectorContribution>
                            {
                                new DiscoveryVectorContribution(
                                    request.Input,
                                    "social-chocolate",
                                    0.0,
                                    1.0,
                                    discovery.Facts[0]),

                                new DiscoveryVectorContribution(
                                    request.Input,
                                    "personal-vanilla",
                                    1.0,
                                    1.0,
                                    discovery.Facts[0])
                            }));
                }

                throw new InvalidOperationException(
                    "The consumer vectorization service does not " +
                    "recognize this Discovery.");
            }
        }

        private sealed class AbsoluteMagnitudeMetric :
            IDiscoveryVectorMetric
        {
            public double GetMagnitude(
                IDiscoveryVectorPoint point)
            {
                var total =
                    0.0;

                for (var index = 0;
                     index < point.Components.Count;
                     index++)
                {
                    total +=
                        Math.Abs(
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
                {
                    throw new ArgumentNullException(
                        nameof(members));
                }

                if (members.Count == 0)
                {
                    throw new ArgumentException(
                        "A readable cluster requires at least one member.",
                        nameof(members));
                }

                Members =
                    members;

                Centroid =
                    members[0];

                Density =
                    1.0;

                Dispersion =
                    0.0;

                Coherence =
                    1.0;
            }

            public IReadOnlyList<IDiscoveryVector>
                Members { get; }

            public IDiscoveryVectorPoint
                Centroid { get; }

            public double Density { get; }

            public double Dispersion { get; }

            public double Coherence { get; }
        }

        private sealed class KidFiveTruthPolicy :
            IIdentifiedTruthCongealmentPolicy
        {
            public KidFiveTruthPolicy()
            {
                Id =
                    Guid.NewGuid();
            }

            public Guid Id { get; }

            public string Slug =>
                "kid-five-truth-policy";

            public bool CanCongealCalled
            {
                get;
                private set;
            }

            public bool CongealCalled
            {
                get;
                private set;
            }

            public IDiscoveryCompetingNarrativesResult?
                Narratives { get; private set; }

            public IReadOnlyList<IDiscoveryCluster>?
                Clusters { get; private set; }

            public IDiscoveryVectorSeries?
                Series { get; private set; }

            public bool CanCongeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CanCongealCalled =
                    true;

                Narratives =
                    narratives;

                Clusters =
                    clusters;

                Series =
                    series;

                // Consumer-owned interpretation:
                //
                // Truth may congeal when the two recognized knowledge
                // structures remain separate and the outcome surface
                // retains one cross-group competing relationship.
                return
                    narratives.Composition.Groups.Count == 2 &&
                    narratives.GroupRelationships.Count == 1 &&
                    narratives
                        .GroupRelationships[0]
                        .Definitions
                        .Count == 1;
            }

            public ITruth Congeal(
                IDiscoveryCompetingNarrativesResult narratives,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IDiscoveryVectorSeries series)
            {
                CongealCalled =
                    true;

                Narratives =
                    narratives;

                Clusters =
                    clusters;

                Series =
                    series;

                var discoveries =
                    new List<IDiscovery>();

                for (var index = 0;
                     index < series.Inputs.Count;
                     index++)
                {
                    discoveries.Add(
                        series.Inputs[index].Discovery);
                }

                return new KidFiveTruth(
                    series,
                    clusters,
                    discoveries);
            }
        }

        private sealed class KidFiveTruth :
            ITruth
        {
            public KidFiveTruth(
                IDiscoveryVectorSeries series,
                IReadOnlyList<IDiscoveryCluster> clusters,
                IReadOnlyList<IDiscovery> discoveries)
            {
                Id =
                    Guid.NewGuid();

                Slug =
                    "kid-five-social-and-personal-truth";

                Name =
                    "KidFiveUnderstandsSocialAndPersonalPreference";

                Value =
                    "Chocolate is socially dominant while " +
                    "Kid 5 still personally prefers vanilla.";

                Series =
                    series ??
                    throw new ArgumentNullException(
                        nameof(series));

                Clusters =
                    clusters ??
                    throw new ArgumentNullException(
                        nameof(clusters));

                Discoveries =
                    discoveries ??
                    throw new ArgumentNullException(
                        nameof(discoveries));

                Stability =
                    1.0;

                Confidence =
                    1.0;

                CongealedAt =
                    DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }

            public string Slug { get; }

            public string Name { get; }

            public object? Value { get; }

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

        private sealed class KidFiveConsequencePolicy :
            IIdentifiedConsequenceProjectionPolicy
        {
            public KidFiveConsequencePolicy()
            {
                Id =
                    Guid.NewGuid();
            }

            public Guid Id { get; }

            public string Slug =>
                "kid-five-consequence-policy";

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

                if (truths == null)
                {
                    throw new ArgumentNullException(
                        nameof(truths));
                }

                if (truths.Count == 0)
                {
                    return new List<IConsequence>();
                }

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
                Id =
                    Guid.NewGuid();

                Slug =
                    "kid-five-may-choose-vanilla";

                Name =
                    "KidFiveMayChooseVanilla";

                Value =
                    "Kid 5 may choose vanilla despite surrounding " +
                    "chocolate preference.";

                Truths =
                    new List<ITruth>
                    {
                        truth ??
                        throw new ArgumentNullException(
                            nameof(truth))
                    };

                Magnitude =
                    1.0;

                Confidence =
                    1.0;

                ProjectedAt =
                    DateTimeOffset.UtcNow;
            }

            public Guid Id { get; }

            public string Slug { get; }

            public string Name { get; }

            public object? Value { get; }

            public IReadOnlyList<ITruth>
                Truths { get; }

            public double Magnitude { get; }

            public double Confidence { get; }

            public DateTimeOffset
                ProjectedAt { get; }
        }
    }
}