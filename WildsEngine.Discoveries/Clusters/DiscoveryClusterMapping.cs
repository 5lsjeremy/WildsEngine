using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterMapping : IdentifierBase, IDiscoveryClusterMapping
    {
        public DiscoveryClusterMapping(
            Guid id, string slug, Guid documentId, string documentSlug,
            string documentVersion, string semanticKey, string geometryType,
            IReadOnlyDictionary<string, double> centroid, double radius,
            IReadOnlyDictionary<string, string>? metadata = null)
            : base(id, slug)
        {
            if (documentId == Guid.Empty) throw new ArgumentException("Document ID is required.", nameof(documentId));
            if (string.IsNullOrWhiteSpace(documentSlug)) throw new ArgumentException("Document slug is required.", nameof(documentSlug));
            if (string.IsNullOrWhiteSpace(documentVersion)) throw new ArgumentException("Document version is required.", nameof(documentVersion));
            if (string.IsNullOrWhiteSpace(semanticKey)) throw new ArgumentException("Semantic key is required.", nameof(semanticKey));
            if (!string.Equals(geometryType, "hypersphere", StringComparison.Ordinal))
                throw new ArgumentException("Only hypersphere geometry is supported by H3A.", nameof(geometryType));
            if (centroid == null || centroid.Count == 0) throw new ArgumentException("Centroid dimensions are required.", nameof(centroid));
            if (double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(radius));
            var centroidCopy = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var pair in centroid)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)) throw new ArgumentException("Centroid dimension names cannot be empty.", nameof(centroid));
                if (double.IsNaN(pair.Value) || double.IsInfinity(pair.Value)) throw new ArgumentOutOfRangeException(nameof(centroid));
                centroidCopy.Add(pair.Key.Trim(), pair.Value);
            }
            var metadataCopy = new Dictionary<string, string>(StringComparer.Ordinal);
            if (metadata != null) foreach (var pair in metadata) metadataCopy.Add(pair.Key, pair.Value);
            DocumentId = documentId;
            DocumentSlug = documentSlug.Trim();
            DocumentVersion = documentVersion.Trim();
            SemanticKey = semanticKey.Trim();
            GeometryType = geometryType;
            Centroid = new ReadOnlyDictionary<string, double>(centroidCopy);
            Radius = radius;
            Metadata = new ReadOnlyDictionary<string, string>(metadataCopy);
        }
        public Guid DocumentId { get; }
        public string DocumentSlug { get; }
        public string DocumentVersion { get; }
        public string SemanticKey { get; }
        public string GeometryType { get; }
        public IReadOnlyDictionary<string, double> Centroid { get; }
        public double Radius { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
