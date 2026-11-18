using System;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public sealed class DiscoveryClusterMappingImportRequest
    {
        public DiscoveryClusterMappingImportRequest(string sourceName, string documentContent)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentException("Source name is required.", nameof(sourceName));
            if (string.IsNullOrWhiteSpace(documentContent))
                throw new ArgumentException("Mapping document content is required.", nameof(documentContent));
            SourceName = sourceName.Trim();
            DocumentContent = documentContent;
        }
        public string SourceName { get; }
        public string DocumentContent { get; }
    }
}
