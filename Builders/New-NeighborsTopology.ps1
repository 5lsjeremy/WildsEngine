param([string]$Root,[switch]$Force)
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($Root)) { $Root=(Get-Location).Path }
if (Test-Path (Join-Path $Root "WildsEngine.Core.csproj")) { $CoreRoot=$Root }
elseif (Test-Path (Join-Path $Root "WildsEngine.Core\WildsEngine.Core.csproj")) { $CoreRoot=Join-Path $Root "WildsEngine.Core" }
else { throw "Run from solution root or WildsEngine.Core, or pass -Root." }
function Put([string]$Rel,[string]$Text){$p=Join-Path $CoreRoot $Rel;$d=Split-Path $p -Parent;New-Item -ItemType Directory -Force -Path $d|Out-Null;if((Test-Path $p)-and-not $Force){Write-Host "[SKIP] $Rel" -ForegroundColor Yellow;return};Set-Content $p $Text -Encoding UTF8;Write-Host "[WRITE] $Rel" -ForegroundColor Green}

Put "Abstractions\Topology\INeighborTraversal.cs" @'
using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborTraversal
    {
        IReadOnlyList<IBaseObject> Traverse(IBaseObject origin, string membership, double maximumDistance);
        bool IsReachable(IBaseObject origin, IBaseObject target, string membership, double maximumDistance);
    }
}
'@
Put "Abstractions\Topology\INeighborCluster.cs" @'
using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborCluster
    {
        string Membership { get; }
        IReadOnlyList<IBaseObject> Members { get; }
    }
}
'@
Put "Abstractions\Topology\INeighborClusterer.cs" @'
using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborClusterer
    {
        IReadOnlyList<INeighborCluster> FindClusters(IReadOnlyList<IBaseObject> objects, string membership);
    }
}
'@
Put "Abstractions\Topology\INeighborRoute.cs" @'
using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborRoute
    {
        string Membership { get; }
        IReadOnlyList<IBaseObject> Nodes { get; }
        double Distance { get; }
        int Hops { get; }
    }
}
'@
Put "Abstractions\Topology\INeighborRouter.cs" @'
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborRouter
    {
        bool TryFindRoute(IBaseObject origin, IBaseObject target, string membership, out INeighborRoute? route);
    }
}
'@
Put "Abstractions\Topology\INeighborPropagation.cs" @'
using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborPropagation<TResult>
    {
        IReadOnlyList<TResult> Propagate(IBaseObject origin, string membership, double maximumDistance);
    }
}
'@
Put "Abstractions\Topology\INeighborPropagationRule.cs" @'
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborPropagationRule<TResult>
    {
        TResult Apply(IBaseObject origin, IBaseObject current, INeighborRelationship relationship, double accumulatedDistance);
    }
}
'@

Put "Topology\NeighborCluster.cs" @'
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
'@
Put "Topology\NeighborRoute.cs" @'
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
'@
Put "Topology\NeighborTraversal.cs" @'
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
'@
Put "Topology\NeighborRouter.cs" @'
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
'@
Put "Topology\NeighborClusterer.cs" @'
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
'@
Put "Topology\NeighborPropagation.cs" @'
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
'@

Write-Host "Topology layer created. Run: dotnet build" -ForegroundColor Cyan
