using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborPropagation<TResult> : INeighborPropagation<TResult>
    {
        private readonly INeighborPropagationRule<TResult> _rule;
        public NeighborPropagation(INeighborPropagationRule<TResult> rule){_rule=rule??throw new ArgumentNullException(nameof(rule));}
        public IReadOnlyList<TResult> Propagate(IBaseObject origin,string membership,double maximumDistance)
        {
            if(origin==null)throw new ArgumentNullException(nameof(origin));if(string.IsNullOrWhiteSpace(membership))throw new ArgumentException("Membership cannot be empty.",nameof(membership));membership=membership.Trim();NeighborDistance.Validate(maximumDistance,nameof(maximumDistance));
            var results=new List<TResult>();var best=new Dictionary<Guid,double>();var queue=new List<Entry>();best[origin.Id]=0d;queue.Add(new Entry(origin,0d));while(queue.Count>0){var bi=0;for(var i=1;i<queue.Count;i++)if(queue[i].Distance<queue[bi].Distance)bi=i;var e=queue[bi];queue.RemoveAt(bi);var rels=e.Object.Neighbors.Relationships;for(var i=0;i<rels.Count;i++){var r=rels[i];if(!string.Equals(r.Membership,membership,StringComparison.Ordinal))continue;var next=e.Distance+r.Distance;if(next>maximumDistance)continue;double old;if(best.TryGetValue(r.Neighbor.Id,out old)&&old<=next)continue;best[r.Neighbor.Id]=next;results.Add(_rule.Apply(origin,e.Object,r,next));queue.Add(new Entry(r.Neighbor,next));}}return results.ToArray();
        }
        private sealed class Entry{public Entry(IBaseObject o,double d){Object=o;Distance=d;}public IBaseObject Object{get;}public double Distance{get;}}
    }
}
