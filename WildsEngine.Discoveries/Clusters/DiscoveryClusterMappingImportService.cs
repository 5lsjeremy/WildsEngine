using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Services;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterMappingImportService :
        WildsServiceBase<DiscoveryClusterMappingImportRequest, DiscoveryClusterMappingImportResult>,
        IDiscoveryClusterMappingImportService
    {
        private readonly IDiscoveryClusterMappingRegistry _registry;
        public DiscoveryClusterMappingImportService(
            string slug,
            IDiscoveryClusterMappingRegistry registry,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(slug, contexts, journal, clock)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }
        protected override Task<DiscoveryClusterMappingImportResult> ExecuteCoreAsync(
            DiscoveryClusterMappingImportRequest request,
            IWildsServiceContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = DiscoveryClusterMappingJsonParser.Parse(request.SourceName, request.DocumentContent);
            cancellationToken.ThrowIfCancellationRequested();
            _registry.RegisterDocument(document);
            return Task.FromResult(new DiscoveryClusterMappingImportResult(
                document.Id, document.Slug, document.Mappings.Count));
        }
    }
}
