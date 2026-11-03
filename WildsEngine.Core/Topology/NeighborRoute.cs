using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborRoute : INeighborRoute
    {
        public NeighborRoute(string membership, IReadOnlyList<IBaseObject> nodes, double distance)
        {
            if(string.IsNullOrWhiteSpace(membership)) throw new ArgumentException("Membership cannot be empty.",nameof(membership)); if(nodes==null) throw new ArgumentNullException(nameof(nodes)); if(nodes.Count==0) throw new ArgumentException("Route requires at least one node.",nameof(nodes)); Membership=membership.Trim(); Distance=NeighborDistance.Validate(distance,nameof(distance)); var copy=new IBaseObject[nodes.Count]; for(var i=0;i<nodes.Count;i++) copy[i]=nodes[i] ?? throw new ArgumentException("Route nodes cannot contain null.",nameof(nodes)); Nodes=copy;
        }
        public string Membership { get; }
        public IReadOnlyList<IBaseObject> Nodes { get; }
        public double Distance { get; }
        public int Hops => Nodes.Count - 1;
    }
}
