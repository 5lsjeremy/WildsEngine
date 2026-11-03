using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborClusterer : INeighborClusterer
    {
        public IReadOnlyList<INeighborCluster> FindClusters(IReadOnlyList<IBaseObject> objects,string membership)
        {
            if(objects==null)throw new ArgumentNullException(nameof(objects));if(string.IsNullOrWhiteSpace(membership))throw new ArgumentException("Membership cannot be empty.",nameof(membership));membership=membership.Trim();
            var known=new Dictionary<Guid,IBaseObject>();for(var i=0;i<objects.Count;i++){var o=objects[i]??throw new ArgumentException("Objects cannot contain null.",nameof(objects));known[o.Id]=o;}var visited=new HashSet<Guid>();var clusters=new List<INeighborCluster>();
            foreach(var pair in known){if(visited.Contains(pair.Key))continue;var members=new List<IBaseObject>();var queue=new Queue<IBaseObject>();queue.Enqueue(pair.Value);visited.Add(pair.Key);while(queue.Count>0){var current=queue.Dequeue();members.Add(current);foreach(var candidate in known.Values){if(visited.Contains(candidate.Id))continue;if(Connected(current,candidate,membership)){visited.Add(candidate.Id);queue.Enqueue(candidate);}}}members.Sort((a,b)=>a.Id.CompareTo(b.Id));clusters.Add(new NeighborCluster(membership,members.ToArray()));}
            return clusters.ToArray();
        }
        private static bool Connected(IBaseObject a,IBaseObject b,string membership){return Has(a,b.Id,membership)||Has(b,a.Id,membership);}
        private static bool Has(IBaseObject a,Guid id,string membership){var r=a.Neighbors.Relationships;for(var i=0;i<r.Count;i++)if(r[i].Neighbor.Id==id&&string.Equals(r[i].Membership,membership,StringComparison.Ordinal))return true;return false;}
    }
}
