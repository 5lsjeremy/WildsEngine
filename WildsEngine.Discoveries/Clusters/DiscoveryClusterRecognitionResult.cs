using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterRecognitionResult : IDiscoveryClusterRecognitionResult
    {
        public DiscoveryClusterRecognitionResult(IDiscoveryClusterPersistenceResult persistence,IDiscoveryClusterRecognitionPolicy policy,DateTimeOffset evaluatedAt)
        { Persistence=persistence??throw new ArgumentNullException(nameof(persistence));Mapping=persistence.Mapping;if(policy==null)throw new ArgumentNullException(nameof(policy));RecognitionPolicyId=policy.Id;RecognitionPolicySlug=policy.Slug;EvaluatedAt=evaluatedAt;IsRecognized=policy.Recognizes(persistence); }
        public IDiscoveryClusterMapping Mapping{get;} public IDiscoveryClusterPersistenceResult Persistence{get;} public bool IsRecognized{get;} public Guid RecognitionPolicyId{get;} public string RecognitionPolicySlug{get;} public DateTimeOffset EvaluatedAt{get;}
    }
}
