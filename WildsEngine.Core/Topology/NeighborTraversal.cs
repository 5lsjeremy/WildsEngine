using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborTraversal : INeighborTraversal
    {
        public IReadOnlyList<IBaseObject> Traverse(IBaseObject origin,string membership,double maximumDistance)
        {
            if(origin==null) throw new ArgumentNullException(nameof(origin)); membership=Normalize(membership); NeighborDistance.Validate(maximumDistance,nameof(maximumDistance));
            var result=new List<IBaseObject>(); var best=new Dictionary<Guid,double>(); var queue=new List<Entry>(); best[origin.Id]=0d; queue.Add(new Entry(origin,0d));
            while(queue.Count>0){var bestIndex=0;for(var i=1;i<queue.Count;i++)if(Compare(queue[i],queue[bestIndex])<0)bestIndex=i;var entry=queue[bestIndex];queue.RemoveAt(bestIndex);double known;if(!best.TryGetValue(entry.Object.Id,out known)||entry.Distance!=known)continue;if(entry.Object.Id!=origin.Id)result.Add(entry.Object);var rels=entry.Object.Neighbors.Relationships;for(var i=0;i<rels.Count;i++){var r=rels[i];if(!string.Equals(r.Membership,membership,StringComparison.Ordinal))continue;var next=entry.Distance+r.Distance;if(next>maximumDistance)continue;double old;if(best.TryGetValue(r.Neighbor.Id,out old)&&old<=next)continue;best[r.Neighbor.Id]=next;queue.Add(new Entry(r.Neighbor,next));}}
            return result.ToArray();
        }
        public bool IsReachable(IBaseObject origin,IBaseObject target,string membership,double maximumDistance){if(target==null)throw new ArgumentNullException(nameof(target));if(origin!=null&&origin.Id==target.Id)return true;var nodes=Traverse(origin,membership,maximumDistance);for(var i=0;i<nodes.Count;i++)if(nodes[i].Id==target.Id)return true;return false;}
        private static string Normalize(string membership){if(string.IsNullOrWhiteSpace(membership))throw new ArgumentException("Membership cannot be empty.",nameof(membership));return membership.Trim();}
        private static int Compare(Entry a,Entry b){var x=a.Distance.CompareTo(b.Distance);return x!=0?x:a.Object.Id.CompareTo(b.Object.Id);}
        private sealed class Entry{public Entry(IBaseObject o,double d){Object=o;Distance=d;}public IBaseObject Object{get;}public double Distance{get;}}
    }
}
