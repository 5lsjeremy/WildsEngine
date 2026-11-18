using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Surface
{
    internal sealed class DiscoveryNarrativeSurface : IDiscoveryNarrativeSurface
    {
        private readonly IDiscoveryGeometrySurface _geometry;
        private readonly IDiscoveryRecognizedGeometryPopulationFactory _populations;

        public DiscoveryNarrativeSurface(
            IDiscoveryGeometrySurface geometry,
            IDiscoveryRecognizedGeometryPopulationFactory populations)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _populations = populations ?? throw new ArgumentNullException(nameof(populations));
        }

        public IDiscoveryRecognizedGeometryPopulation CreatePopulation(
            Guid geometryRegistrationId,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> recognizedGeometry)
        {
            if (!_geometry.TryGetRegistration(geometryRegistrationId, out var registration) || registration == null)
                throw new KeyNotFoundException("No geometry registration exists for ID: " + geometryRegistrationId);
            return _populations.Create(registration, recognizedGeometry);
        }
    }
}
