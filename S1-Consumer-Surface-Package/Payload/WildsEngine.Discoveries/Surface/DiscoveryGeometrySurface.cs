using System;
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
    internal sealed class DiscoveryGeometrySurface : IDiscoveryGeometrySurface
    {
        private readonly IDiscoveryClusterMappingRegistry _mappings;
        private readonly IDiscoveryClusterMappingImportService _imports;
        private readonly IDiscoveryClusterRelationshipProjector _projector;
        private readonly IWildsServiceClock _clock;
        private readonly DiscoveryGeometryRegistrationCatalog _registrations;

        public DiscoveryGeometrySurface(
            IDiscoveryClusterMappingRegistry mappings,
            IDiscoveryClusterMappingImportService imports,
            IDiscoveryClusterRelationshipProjector projector,
            IWildsServiceClock clock,
            DiscoveryGeometryRegistrationCatalog registrations)
        {
            _mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
            _imports = imports ?? throw new ArgumentNullException(nameof(imports));
            _projector = projector ?? throw new ArgumentNullException(nameof(projector));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
        }

        public IDiscoveryGeometryRegistration Register(IDiscoveryGeometryProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var document = new DiscoveryClusterMappingDocument(profile.Id, profile.Slug, profile.Version, "programmatic-profile", profile.Mappings);
            _mappings.RegisterDocument(document);
            return RegisterSnapshot(profile.Id, profile.Slug, profile.Version, profile.Scope, profile.Mappings);
        }

        public async Task<IDiscoveryGeometryRegistration> RegisterDocumentAsync(
            string sourceName,
            string documentContent,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _imports.ExecuteAsync(
                new DiscoveryClusterMappingImportRequest(sourceName, documentContent),
                provenance,
                parentExecutionId,
                pulseId,
                cancellationToken);
            var mappings = _mappings.GetByDocument(result.Value.DocumentId);
            if (mappings.Count == 0) throw new InvalidOperationException("The imported mapping document produced no registered mappings.");
            return RegisterSnapshot(
                result.Value.DocumentId,
                result.Value.DocumentSlug,
                mappings[0].DocumentVersion,
                result.Value.DocumentSlug,
                mappings);
        }

        public IReadOnlyList<IDiscoveryGeometryRegistration> Registrations => _registrations.All;
        public bool TryGetRegistration(Guid registrationId, out IDiscoveryGeometryRegistration? registration) => _registrations.TryGet(registrationId, out registration);
        public bool TryGetMapping(string semanticKey, out IDiscoveryClusterMapping? mapping) => _mappings.TryGetBySemanticKey(semanticKey, out mapping);

        public IDiscoveryClusterSpatialRelationship Locate(IDiscoveryVectorEvaluation evaluation, string semanticKey)
        {
            if (!_mappings.TryGetBySemanticKey(semanticKey, out var mapping) || mapping == null)
                throw new KeyNotFoundException("No cluster mapping is registered for semantic key: " + semanticKey);
            return _projector.Locate(evaluation, mapping);
        }

        public IDiscoveryClusterTrajectoryProjection Project(IDiscoveryVectorMotionResult motion, string semanticKey)
        {
            if (!_mappings.TryGetBySemanticKey(semanticKey, out var mapping) || mapping == null)
                throw new KeyNotFoundException("No cluster mapping is registered for semantic key: " + semanticKey);
            return _projector.Project(motion, mapping);
        }

        private IDiscoveryGeometryRegistration RegisterSnapshot(
            Guid profileId,
            string profileSlug,
            string profileVersion,
            string scope,
            IReadOnlyList<IDiscoveryClusterMapping> mappings)
        {
            var ids = new Guid[mappings.Count];
            for (var index = 0; index < mappings.Count; index++) ids[index] = mappings[index].Id;
            var id = Guid.NewGuid();
            var registration = new DiscoveryGeometryRegistration(
                id,
                "geometry-registration-" + id.ToString("N"),
                profileId,
                profileSlug,
                profileVersion,
                scope,
                _clock.GetUtcNow(),
                ids);
            _registrations.Register(registration);
            return registration;
        }
    }
}
