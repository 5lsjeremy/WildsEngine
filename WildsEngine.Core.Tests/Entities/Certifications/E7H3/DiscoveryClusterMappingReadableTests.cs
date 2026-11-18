using System;
using System.IO;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Registries;
using WildsEngine.Discoveries.Services;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterMappingReadableTests
    {
        [Fact]
        public async Task UserMappingDocumentBecomesQueryableClusterRegistryData()
        {
            TestLog.Section("E7-H3A - USER CLUSTER MAPPING IMPORT");
            var clock = new SystemWildsServiceClock();
            var registry = new DiscoveryClusterMappingRegistry();
            var journal = new WildsServiceExecutionJournal();
            var service = new DiscoveryClusterMappingImportService(
                "cluster-mapping-import", registry,
                new WildsServiceContextFactory(clock), journal, clock);
            var path =
                Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "..",
                        "..",
                        "..",
                        "Entities",
                        "Certifications",
                        "E7H3",
                        "Examples",
                        "industrial-condition-regions.json"));

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "The E7-H3 cluster mapping readable example was not found.",
                    path);
            }
            var result = await service.ExecuteAsync(
                new DiscoveryClusterMappingImportRequest(path, File.ReadAllText(path)),
                provenance: "consumer supplied mapping document");
            Assert.True(registry.TryGetBySemanticKey("MaintenanceConcern", out var mapping));
            TestLog.State($"Document = {result.Value.DocumentSlug} ({result.Value.DocumentId})");
            TestLog.State($"Imported mappings = {result.Value.ImportedCount}");
            TestLog.State($"Execution = {result.ExecutionId}");
            TestLog.State($"Semantic key = {mapping!.SemanticKey}");
            TestLog.State($"Geometry = {mapping.GeometryType}; radius={mapping.Radius:R}");
            TestLog.State($"Centroid dimensions = {mapping.Centroid.Count}");
            Assert.Equal("Maintenance Concern", mapping.Metadata["displayName"]);
            TestLog.Pass("The consumer mapping document was validated by an attributable async service and stored atomically in the cluster mapping registry.");
        }
    }
}
