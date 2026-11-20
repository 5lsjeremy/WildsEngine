using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Surface;
using WildsEngine.Discoveries.Clusters;

namespace WildsEngine.Discoveries.Surface
{
    internal sealed class DiscoveryGeometrySurface :
        IDiscoveryGeometrySurface
    {
        private readonly IDiscoveryClusterMappingRegistry
            _mappings;

        private readonly IDiscoveryClusterMappingImportService
            _imports;

        private readonly IDiscoveryClusterRelationshipProjector
            _projector;

        private readonly IWildsServiceClock
            _clock;

        private readonly DiscoveryGeometryRegistrationCatalog
            _registrations;

        private readonly ConcurrentDictionary<
            string,
            IDiscoveryGeometryProfile> _profiles =
                new ConcurrentDictionary<
                    string,
                    IDiscoveryGeometryProfile>(
                        StringComparer.Ordinal);

        private readonly ConcurrentDictionary<
            ObservationHistoryKey,
            List<IDiscoveryClusterObservation>> _observations =
                new ConcurrentDictionary<
                    ObservationHistoryKey,
                    List<IDiscoveryClusterObservation>>();

        public DiscoveryGeometrySurface(
            IDiscoveryClusterMappingRegistry mappings,
            IDiscoveryClusterMappingImportService imports,
            IDiscoveryClusterRelationshipProjector projector,
            IWildsServiceClock clock,
            DiscoveryGeometryRegistrationCatalog registrations)
        {
            _mappings =
                mappings ??
                throw new ArgumentNullException(
                    nameof(mappings));

            _imports =
                imports ??
                throw new ArgumentNullException(
                    nameof(imports));

            _projector =
                projector ??
                throw new ArgumentNullException(
                    nameof(projector));

            _clock =
                clock ??
                throw new ArgumentNullException(
                    nameof(clock));

            _registrations =
                registrations ??
                throw new ArgumentNullException(
                    nameof(registrations));
        }

        public IDiscoveryGeometryRegistration Register(
            IDiscoveryGeometryProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(
                    nameof(profile));
            }

            var document =
                new DiscoveryClusterMappingDocument(
                    profile.Id,
                    profile.Slug,
                    profile.Version,
                    "programmatic-profile",
                    profile.Mappings);

            _mappings.RegisterDocument(
                document);

            foreach (var mapping in profile.Mappings)
            {
                if (!_profiles.TryAdd(
                        mapping.SemanticKey,
                        profile))
                {
                    throw new InvalidOperationException(
                        "A geometry profile already owns semantic key: " +
                        mapping.SemanticKey);
                }
            }

            return RegisterSnapshot(
                profile.Id,
                profile.Slug,
                profile.Version,
                profile.Scope,
                profile.Mappings);
        }

        public async Task<IDiscoveryGeometryRegistration>
            RegisterDocumentAsync(
                string sourceName,
                string documentContent,
                object? provenance = null,
                Guid? parentExecutionId = null,
                Guid? pulseId = null,
                CancellationToken cancellationToken = default)
        {
            var result =
                await _imports.ExecuteAsync(
                    new DiscoveryClusterMappingImportRequest(
                        sourceName,
                        documentContent),
                    provenance,
                    parentExecutionId,
                    pulseId,
                    cancellationToken);

            var maps =
                _mappings.GetByDocument(
                    result.Value.DocumentId);

            if (maps.Count == 0)
            {
                throw new InvalidOperationException(
                    "The imported mapping document produced " +
                    "no registered mappings.");
            }

            return RegisterSnapshot(
                result.Value.DocumentId,
                result.Value.DocumentSlug,
                maps[0].DocumentVersion,
                result.Value.DocumentSlug,
                maps);
        }

        public IReadOnlyList<IDiscoveryGeometryRegistration>
            Registrations =>
                _registrations.All;

        public bool TryGetRegistration(
            Guid registrationId,
            out IDiscoveryGeometryRegistration? registration)
        {
            return _registrations.TryGet(
                registrationId,
                out registration);
        }

        public bool TryGetMapping(
            string semanticKey,
            out IDiscoveryClusterMapping? mapping)
        {
            return _mappings.TryGetBySemanticKey(
                semanticKey,
                out mapping);
        }

        public IDiscoveryClusterSpatialRelationship Locate(
            IDiscoveryVectorEvaluation evaluation,
            string semanticKey)
        {
            return _projector.Locate(
                evaluation,
                Mapping(
                    semanticKey));
        }

        public IDiscoveryClusterTrajectoryProjection Project(
            IDiscoveryVectorMotionResult motion,
            string semanticKey)
        {
            return _projector.Project(
                motion,
                Mapping(
                    semanticKey));
        }

        public IDiscoveryGeometryObservationResult Observe(
            IDiscoveryVectorEvaluation evaluation,
            string semanticKey,
            string historyKey)
        {
            if (evaluation == null)
            {
                throw new ArgumentNullException(
                    nameof(evaluation));
            }

            if (string.IsNullOrWhiteSpace(historyKey))
            {
                throw new ArgumentException(
                    "Observation history key cannot be empty.",
                    nameof(historyKey));
            }

            if (!_profiles.TryGetValue(
                    semanticKey,
                    out var profile) ||
                profile.PersistencePolicy == null ||
                profile.StabilityMetric == null ||
                profile.RecognitionPolicy == null)
            {
                throw new InvalidOperationException(
                    "The registered geometry profile does not configure " +
                    "persistence and recognition for: " +
                    semanticKey);
            }

            var spatial =
                Locate(
                    evaluation,
                    semanticKey);

            var history =
                new ObservationHistoryKey(
                    historyKey,
                    spatial.Mapping.Id);

            var observation =
                new DiscoveryClusterObservation(
                    spatial);

            var observations =
                _observations.GetOrAdd(
                    history,
                    _ =>
                        new List<
                            IDiscoveryClusterObservation>());

            IDiscoveryClusterObservation[] snapshot;

            lock (observations)
            {
                if (observations.Count > 0 &&
                    observation.ObservedAt <=
                    observations[
                        observations.Count - 1]
                        .ObservedAt)
                {
                    throw new InvalidOperationException(
                        "Observations must arrive in strictly " +
                        "increasing evaluation time within one " +
                        "observation history.");
                }

                observations.Add(
                    observation);

                snapshot =
                    observations.ToArray();
            }

            var stabilityObservations =
                GetCurrentOccupancyObservations(
                    snapshot);

            var stability =
                profile.StabilityMetric.GetStability(
                    stabilityObservations);

            var persistence =
                new DiscoveryClusterPersistenceResult(
                    spatial.Mapping,
                    snapshot,
                    stability,
                    profile.PersistencePolicy,
                    profile.StabilityMetric);

            var recognition =
                new DiscoveryClusterRecognitionResult(
                    persistence,
                    profile.RecognitionPolicy,
                    _clock.GetUtcNow());

            return new DiscoveryGeometryObservationResult(
                spatial,
                persistence,
                recognition);
        }

        private IDiscoveryClusterMapping Mapping(
            string semanticKey)
        {
            if (!_mappings.TryGetBySemanticKey(
                    semanticKey,
                    out var mapping) ||
                mapping == null)
            {
                throw new KeyNotFoundException(
                    "No cluster mapping is registered for semantic key: " +
                    semanticKey);
            }

            return mapping;
        }

        private IDiscoveryGeometryRegistration RegisterSnapshot(
            Guid profileId,
            string slug,
            string version,
            string scope,
            IReadOnlyList<IDiscoveryClusterMapping> maps)
        {
            var ids =
                new Guid[maps.Count];

            for (var index = 0;
                 index < maps.Count;
                 index++)
            {
                ids[index] =
                    maps[index].Id;
            }

            var id =
                Guid.NewGuid();

            var registration =
                new DiscoveryGeometryRegistration(
                    id,
                    "geometry-registration-" +
                    id.ToString("N"),
                    profileId,
                    slug,
                    version,
                    scope,
                    _clock.GetUtcNow(),
                    ids);

            _registrations.Register(
                registration);

            return registration;
        }

        private static IReadOnlyList<
            IDiscoveryClusterObservation>
            GetCurrentOccupancyObservations(
                IReadOnlyList<
                    IDiscoveryClusterObservation> observations)
        {
            if (observations == null)
            {
                throw new ArgumentNullException(
                    nameof(observations));
            }

            if (observations.Count == 0)
            {
                throw new ArgumentException(
                    "At least one observation is required.",
                    nameof(observations));
            }

            var current =
                observations[
                    observations.Count - 1];

            var currentIsInside =
                current.Relationship.IsInside ||
                current.Relationship.IsOnBoundary;

            // There is no active occupancy interval.
            //
            // Preserve the current observation for the metric rather
            // than treating previous occupancy as current stability.
            if (!currentIsInside)
            {
                return new[]
                {
                    current
                };
            }

            var startIndex =
                observations.Count - 1;

            while (startIndex > 0)
            {
                var previous =
                    observations[
                        startIndex - 1];

                var previousIsInside =
                    previous.Relationship.IsInside ||
                    previous.Relationship.IsOnBoundary;

                if (!previousIsInside)
                {
                    break;
                }

                startIndex--;
            }

            var result =
                new List<IDiscoveryClusterObservation>(
                    observations.Count - startIndex);

            for (var index = startIndex;
                 index < observations.Count;
                 index++)
            {
                result.Add(
                    observations[index]);
            }

            return result.AsReadOnly();
        }

        private readonly struct ObservationHistoryKey :
            IEquatable<ObservationHistoryKey>
        {
            public ObservationHistoryKey(
                string historyKey,
                Guid mappingId)
            {
                if (string.IsNullOrWhiteSpace(historyKey))
                {
                    throw new ArgumentException(
                        "Observation history key cannot be empty.",
                        nameof(historyKey));
                }

                if (mappingId == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Observation history requires a mapping identity.",
                        nameof(mappingId));
                }

                HistoryKey =
                    historyKey.Trim();

                MappingId =
                    mappingId;
            }

            public string HistoryKey { get; }

            public Guid MappingId { get; }

            public bool Equals(
                ObservationHistoryKey other)
            {
                return
                    MappingId == other.MappingId &&
                    string.Equals(
                        HistoryKey,
                        other.HistoryKey,
                        StringComparison.Ordinal);
            }

            public override bool Equals(
                object? obj)
            {
                return
                    obj is ObservationHistoryKey other &&
                    Equals(
                        other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return
                        (StringComparer.Ordinal.GetHashCode(
                            HistoryKey) * 397) ^
                        MappingId.GetHashCode();
                }
            }
        }
    }
}