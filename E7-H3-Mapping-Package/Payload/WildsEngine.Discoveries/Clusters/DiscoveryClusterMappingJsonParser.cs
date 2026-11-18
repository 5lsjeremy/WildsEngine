using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using WildsEngine.Discoveries.Abstractions.Clusters;

namespace WildsEngine.Discoveries.Clusters
{
    internal static class DiscoveryClusterMappingJsonParser
    {
        public static IDiscoveryClusterMappingDocument Parse(string sourceName, string content)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(
                    typeof(DocumentDto),
                    new DataContractJsonSerializerSettings
                    {
                        UseSimpleDictionaryFormat = true
                    });
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var dto = serializer.ReadObject(stream) as DocumentDto ?? throw new SerializationException("Document is empty.");
                var documentId = ParseGuid(dto.Id, "document id");
                if (string.IsNullOrWhiteSpace(dto.Slug)) throw new SerializationException("Document slug is required.");
                if (string.IsNullOrWhiteSpace(dto.Version)) throw new SerializationException("Document version is required.");
                if (dto.Mappings == null || dto.Mappings.Count == 0) throw new SerializationException("At least one mapping is required.");
                var mappings = new List<IDiscoveryClusterMapping>(dto.Mappings.Count);
                foreach (var item in dto.Mappings)
                {
                    if (item == null) throw new SerializationException("Mapping cannot be null.");
                    mappings.Add(new DiscoveryClusterMapping(
                        ParseGuid(item.Id, "cluster id"), item.Slug ?? string.Empty,
                        documentId, dto.Slug, dto.Version,
                        item.SemanticKey ?? string.Empty,
                        item.GeometryType ?? string.Empty,
                        item.Centroid ?? new Dictionary<string, double>(),
                        item.Radius,
                        item.Metadata));
                }
                return new DiscoveryClusterMappingDocument(documentId, dto.Slug, dto.Version, sourceName, mappings);
            }
            catch (Exception exception) when (!(exception is ArgumentException))
            {
                throw new ArgumentException("Cluster mapping document is invalid.", nameof(content), exception);
            }
        }
        private static Guid ParseGuid(string? value, string field)
        {
            if (!Guid.TryParse(value, out var parsed) || parsed == Guid.Empty)
                throw new SerializationException("Invalid " + field + ".");
            return parsed;
        }

        [DataContract]
        private sealed class DocumentDto
        {
            [DataMember(Name="id")] public string? Id { get; set; }
            [DataMember(Name="slug")] public string? Slug { get; set; }
            [DataMember(Name="version")] public string? Version { get; set; }
            [DataMember(Name="mappings")] public List<MappingDto>? Mappings { get; set; }
        }
        [DataContract]
        private sealed class MappingDto
        {
            [DataMember(Name="id")] public string? Id { get; set; }
            [DataMember(Name="slug")] public string? Slug { get; set; }
            [DataMember(Name="semanticKey")] public string? SemanticKey { get; set; }
            [DataMember(Name="geometryType")] public string? GeometryType { get; set; }
            [DataMember(Name="centroid")] public Dictionary<string,double>? Centroid { get; set; }
            [DataMember(Name="radius")] public double Radius { get; set; }
            [DataMember(Name="metadata")] public Dictionary<string,string>? Metadata { get; set; }
        }
    }
}
