using System;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Surface;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Registries;
using WildsEngine.Discoveries.Services;

namespace WildsEngine.Discoveries.Surface
{
    public sealed class WildsDiscoveries : IWildsDiscoveries
    {
        public WildsDiscoveries(WildsDiscoveriesOptions? options = null)
        {
            Options = options ?? new WildsDiscoveriesOptions();
            var clock = new SystemWildsServiceClock();
            var journal = new WildsServiceExecutionJournal();
            var contexts = new WildsServiceContextFactory(clock);
            var mappings = new DiscoveryClusterMappingRegistry();
            var imports = new DiscoveryClusterMappingImportService(
                "cluster-mapping-import",
                mappings,
                contexts,
                journal,
                clock);
            Geometry = new DiscoveryGeometrySurface(
                mappings,
                imports,
                new HypersphereDiscoveryClusterRelationshipProjector(Options.GeometryEpsilon),
                clock,
                new DiscoveryGeometryRegistrationCatalog());
            Executions = journal;
        }
        public WildsDiscoveriesOptions Options { get; }
        public IDiscoveryGeometrySurface Geometry { get; }
        public IWildsServiceExecutionJournal Executions { get; }
    }
}
