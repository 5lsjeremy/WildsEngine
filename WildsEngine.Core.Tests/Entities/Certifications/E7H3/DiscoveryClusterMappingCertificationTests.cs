using System;
using System.IO;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Registries;
using WildsEngine.Discoveries.Services;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterMappingCertificationTests
    {
        [Fact]
        public async Task ExampleDocumentImportsAtomicallyAndIsQueryable()
        {
            var fixture = new Fixture();
            var content = ReadExample();

            var result = await fixture.Service.ExecuteAsync(
                new DiscoveryClusterMappingImportRequest(
                    "industrial-condition-regions.json",
                    content),
                pulseId: Guid.NewGuid());

            Assert.Equal(2, result.Value.ImportedCount);
            Assert.Equal(2, fixture.Registry.Mappings.Count);

            Assert.True(
                fixture.Registry.TryGetBySemanticKey(
                    "MaintenanceConcern",
                    out var mapping));

            Assert.NotNull(mapping);
            Assert.Equal("maintenance-concern", mapping!.Slug);
            Assert.Equal(0.70, mapping.Centroid["mechanical-stress"]);
            Assert.Equal(0.18, mapping.Radius);

            Assert.True(
                fixture.Journal.TryGet(
                    result.ExecutionId,
                    out var execution));

            Assert.NotNull(execution);
            Assert.Equal(
                WildsServiceExecutionOutcome.Succeeded,
                execution!.Outcome);
        }

        [Fact]
        public async Task DuplicateImportFailsWithoutPartialMutation()
        {
            var fixture = new Fixture();

            var request =
                new DiscoveryClusterMappingImportRequest(
                    "example.json",
                    ReadExample());

            await fixture.Service.ExecuteAsync(request);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => fixture.Service.ExecuteAsync(request));

            Assert.Equal(2, fixture.Registry.Mappings.Count);
        }

        [Fact]
        public async Task InvalidDocumentFailsWithoutRegistryMutation()
        {
            var fixture = new Fixture();

            var invalid =
                ReadExample().Replace(
                    "\"radius\": 0.18",
                    "\"radius\": -0.18");

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => fixture.Service.ExecuteAsync(
                    new DiscoveryClusterMappingImportRequest(
                        "invalid.json",
                        invalid)));

            Assert.Empty(fixture.Registry.Mappings);
        }

        [Fact]
        public async Task ImportedMeaningRemainsConsumerOwnedOpaqueMetadata()
        {
            var fixture = new Fixture();

            await fixture.Service.ExecuteAsync(
                new DiscoveryClusterMappingImportRequest(
                    "example.json",
                    ReadExample()));

            Assert.True(
                fixture.Registry.TryGet(
                    "maintenance-concern",
                    out var mapping));

            Assert.NotNull(mapping);
            Assert.Equal(
                "MaintenanceConcern",
                mapping!.SemanticKey);

            Assert.Equal(
                "Maintenance Concern",
                mapping.Metadata["displayName"]);

            Assert.Equal(
                "hypersphere",
                mapping.GeometryType);
        }

        private static string ReadExample()
        {
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
                    "The E7-H3 cluster mapping certification example was not found.",
                    path);
            }

            return File.ReadAllText(path);
        }

        private sealed class Fixture
        {
            public Fixture()
            {
                var clock = new SystemWildsServiceClock();

                Registry =
                    new DiscoveryClusterMappingRegistry();

                Journal =
                    new WildsServiceExecutionJournal();

                Service =
                    new DiscoveryClusterMappingImportService(
                        "cluster-mapping-import",
                        Registry,
                        new WildsServiceContextFactory(clock),
                        Journal,
                        clock);
            }

            public DiscoveryClusterMappingRegistry Registry { get; }

            public WildsServiceExecutionJournal Journal { get; }

            public DiscoveryClusterMappingImportService Service { get; }
        }
    }
}