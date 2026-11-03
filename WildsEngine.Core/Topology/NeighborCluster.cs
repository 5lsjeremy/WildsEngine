using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborCluster : INeighborCluster
    {
        public NeighborCluster(string membership, IReadOnlyList<IBaseObject> members)
        {
            if (string.IsNullOrWhiteSpace(membership)) throw new ArgumentException("Membership cannot be empty.", nameof(membership));
            if (members == null) throw new ArgumentNullException(nameof(members));
            Membership=membership.Trim(); var copy=new IBaseObject[members.Count]; for(var i=0;i<members.Count;i++) copy[i]=members[i] ?? throw new ArgumentException("Cluster members cannot contain null.",nameof(members)); Members=copy;
        }
        public string Membership { get; }
        public IReadOnlyList<IBaseObject> Members { get; }
    }
}
