using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IDiscoveryGeometrySurface
    {
        IDiscoveryGeometryRegistration Register(IDiscoveryGeometryProfile profile);
        Task<IDiscoveryGeometryRegistration> RegisterDocumentAsync(
            string sourceName,
            string documentContent,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default);
        IReadOnlyList<IDiscoveryGeometryRegistration> Registrations { get; }
        bool TryGetRegistration(Guid registrationId, out IDiscoveryGeometryRegistration? registration);
        bool TryGetMapping(string semanticKey, out IDiscoveryClusterMapping? mapping);
        IDiscoveryClusterSpatialRelationship Locate(
            IDiscoveryVectorEvaluation evaluation,
            string semanticKey);
        IDiscoveryClusterTrajectoryProjection Project(
            IDiscoveryVectorMotionResult motion,
            string semanticKey);
    }
}
