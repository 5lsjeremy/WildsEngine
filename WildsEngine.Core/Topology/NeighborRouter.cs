using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
namespace WildsEngine.Core.Topology
{
    public sealed class NeighborRouter : INeighborRouter
    {
        public bool TryFindRoute(IBaseObject origin,IBaseObject target,string membership,out INeighborRoute? route)
        {
            if(origin==null)throw new ArgumentNullException(nameof(origin));if(target==null)throw new ArgumentNullException(nameof(target));if(string.IsNullOrWhiteSpace(membership))throw new ArgumentException("Membership cannot be empty.",nameof(membership));membership=membership.Trim();route=null;
            if(origin.Id==target.Id){route=new NeighborRoute(membership,new[]{origin},0d);return true;}
            var dist=new Dictionary<Guid,double>();var objects=new Dictionary<Guid,IBaseObject>();var previous=new Dictionary<Guid,Guid>();var open=new List<IBaseObject>();dist[origin.Id]=0d;objects[origin.Id]=origin;open.Add(origin);
            while(open.Count>0){var bi=0;for(var i=1;i<open.Count;i++){var c=dist[open[i].Id].CompareTo(dist[open[bi].Id]);if(c<0||(c==0&&open[i].Id.CompareTo(open[bi].Id)<0))bi=i;}var current=open[bi];open.RemoveAt(bi);if(current.Id==target.Id)break;var rels=current.Neighbors.Relationships;for(var i=0;i<rels.Count;i++){var r=rels[i];if(!string.Equals(r.Membership,membership,StringComparison.Ordinal))continue;var nd=dist[current.Id]+r.Distance;double old;if(dist.TryGetValue(r.Neighbor.Id,out old)&&old<=nd)continue;dist[r.Neighbor.Id]=nd;objects[r.Neighbor.Id]=r.Neighbor;previous[r.Neighbor.Id]=current.Id;if(!Contains(open,r.Neighbor.Id))open.Add(r.Neighbor);}}
            double total;if(!dist.TryGetValue(target.Id,out total))return false;var reverse=new List<IBaseObject>();var id=target.Id;reverse.Add(objects[id]);while(id!=origin.Id){id=previous[id];reverse.Add(objects[id]);}reverse.Reverse();route=new NeighborRoute(membership,reverse.ToArray(),total);return true;
        }
        private static bool Contains(List<IBaseObject> items,Guid id){for(var i=0;i<items.Count;i++)if(items[i].Id==id)return true;return false;}
    }
}
