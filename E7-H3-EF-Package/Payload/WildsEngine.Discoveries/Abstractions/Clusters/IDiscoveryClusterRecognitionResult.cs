using System;
namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterRecognitionResult
    {
        IDiscoveryClusterMapping Mapping { get; }
        IDiscoveryClusterPersistenceResult Persistence { get; }
        bool IsRecognized { get; }
        Guid RecognitionPolicyId { get; }
        string RecognitionPolicySlug { get; }
        DateTimeOffset EvaluatedAt { get; }
    }
}
