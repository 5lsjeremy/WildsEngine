using System;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public sealed class DiscoveryClusterMappingImportResult
    {
        public DiscoveryClusterMappingImportResult(Guid documentId, string documentSlug, int importedCount)
        {
            if (documentId == Guid.Empty) throw new ArgumentException("Document ID is required.", nameof(documentId));
            if (string.IsNullOrWhiteSpace(documentSlug)) throw new ArgumentException("Document slug is required.", nameof(documentSlug));
            if (importedCount <= 0) throw new ArgumentOutOfRangeException(nameof(importedCount));
            DocumentId = documentId;
            DocumentSlug = documentSlug;
            ImportedCount = importedCount;
        }
        public Guid DocumentId { get; }
        public string DocumentSlug { get; }
        public int ImportedCount { get; }
    }
}
