using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;
namespace WildsEngine.Discoveries.Policies
{
    public sealed class PersistentClusterRecognitionPolicy : IdentifierBase, IDiscoveryClusterRecognitionPolicy
    {
        public PersistentClusterRecognitionPolicy(string slug="persistent-cluster"):base(Guid.NewGuid(),slug){}
        public bool Recognizes(IDiscoveryClusterPersistenceResult persistence){if(persistence==null)throw new ArgumentNullException(nameof(persistence));return persistence.IsPersistent;}
    }
}
