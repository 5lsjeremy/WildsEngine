using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class HypersphereDiscoveryClusterRelationshipProjector : IDiscoveryClusterRelationshipProjector
    {
        private readonly double _epsilon;
        public HypersphereDiscoveryClusterRelationshipProjector(double epsilon = 1e-10)
        {
            if (double.IsNaN(epsilon) || double.IsInfinity(epsilon) || epsilon < 0.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon));
            _epsilon = epsilon;
        }

        public IDiscoveryClusterSpatialRelationship Locate(
            IDiscoveryVectorEvaluation evaluation,
            IDiscoveryClusterMapping mapping)
        {
            Validate(evaluation, mapping);
            var position = Position(evaluation.Vector, mapping);
            var delta = Delta(position, mapping.Centroid);
            var distance = Magnitude(delta);
            var signed = distance - mapping.Radius;
            var onBoundary = Math.Abs(signed) <= _epsilon;
            var nearest = new Dictionary<string,double>(StringComparer.Ordinal);
            foreach (var pair in mapping.Centroid)
            {
                var direction = distance <= _epsilon ? FirstAxis(pair.Key, mapping.Centroid) : delta[pair.Key] / distance;
                nearest[pair.Key] = pair.Value + (direction * mapping.Radius);
            }
            return new DiscoveryClusterSpatialRelationship(
                mapping, evaluation, distance, signed, signed < -_epsilon, onBoundary, nearest);
        }

        public IDiscoveryClusterTrajectoryProjection Project(
            IDiscoveryVectorMotionResult motion,
            IDiscoveryClusterMapping mapping)
        {
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            var spatial = Locate(motion.Evaluation, mapping);
            var position = Position(motion.Evaluation.Vector, mapping);
            var velocity = Velocity(motion.Motion, mapping);
            var offset = Delta(position, mapping.Centroid);
            var speedSquared = Dot(velocity, velocity);
            var radialRate = spatial.DistanceToCentroid <= _epsilon ? 0.0 : Dot(offset, velocity) / spatial.DistanceToCentroid;
            if (speedSquared <= _epsilon)
                return BuildStationary(spatial, motion, radialRate);

            var closestTime = Math.Max(0.0, -Dot(offset, velocity) / speedSquared);
            var closestPosition = At(position, velocity, closestTime);
            var closestDistance = Magnitude(Delta(closestPosition, mapping.Centroid));

            var b = 2.0 * Dot(offset, velocity);
            var c = Dot(offset, offset) - (mapping.Radius * mapping.Radius);
            var discriminant = (b * b) - (4.0 * speedSquared * c);
            double? entry = null;
            double? exit = null;
            IReadOnlyDictionary<string,double>? entryPoint = null;
            IReadOnlyDictionary<string,double>? exitPoint = null;

            if (discriminant >= -_epsilon)
            {
                discriminant = Math.Max(0.0, discriminant);
                var root = Math.Sqrt(discriminant);
                var t1 = (-b - root) / (2.0 * speedSquared);
                var t2 = (-b + root) / (2.0 * speedSquared);
                if (spatial.IsInside || spatial.IsOnBoundary)
                {
                    entry = 0.0;
                    if (t2 >= 0.0) exit = t2;
                }
                else
                {
                    if (t1 >= 0.0) entry = t1;
                    if (t2 >= 0.0) exit = t2;
                }
                if (entry.HasValue) entryPoint = At(position, velocity, entry.Value);
                if (exit.HasValue) exitPoint = At(position, velocity, exit.Value);
            }

            double? duration = entry.HasValue && exit.HasValue ? Math.Max(0.0, exit.Value - entry.Value) : null;
            var kind = Classify(spatial, radialRate, entry, exit, closestDistance, mapping.Radius);
            return new DiscoveryClusterTrajectoryProjection(
                spatial, motion, kind, radialRate, closestTime, closestDistance,
                closestDistance - mapping.Radius, entry, exit, duration, entryPoint, exitPoint);
        }

        private IDiscoveryClusterTrajectoryProjection BuildStationary(
            IDiscoveryClusterSpatialRelationship spatial,
            IDiscoveryVectorMotionResult motion,
            double radialRate)
        {
            return new DiscoveryClusterTrajectoryProjection(
                spatial, motion,
                spatial.IsInside ? DiscoveryClusterTransitKind.RemainingInside : DiscoveryClusterTransitKind.Stationary,
                radialRate, 0.0, spatial.DistanceToCentroid, spatial.SignedDistanceToBoundary,
                spatial.IsInside ? 0.0 : null, null, null, null, null);
        }

        private DiscoveryClusterTransitKind Classify(
            IDiscoveryClusterSpatialRelationship spatial, double radialRate,
            double? entry, double? exit, double closestDistance, double radius)
        {
            if (spatial.IsInside || spatial.IsOnBoundary)
                return exit.HasValue ? DiscoveryClusterTransitKind.Exiting : DiscoveryClusterTransitKind.RemainingInside;
            if (entry.HasValue && exit.HasValue) return DiscoveryClusterTransitKind.PassingThrough;
            if (Math.Abs(closestDistance - radius) <= _epsilon) return DiscoveryClusterTransitKind.Flyby;
            if (radialRate < -_epsilon) return DiscoveryClusterTransitKind.Approaching;
            if (radialRate > _epsilon) return DiscoveryClusterTransitKind.Receding;
            return DiscoveryClusterTransitKind.Stationary;
        }

        private static void Validate(IDiscoveryVectorEvaluation evaluation, IDiscoveryClusterMapping mapping)
        {
            if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (!string.Equals(mapping.GeometryType, "hypersphere", StringComparison.Ordinal))
                throw new NotSupportedException("This projector supports hypersphere mappings only.");
        }
        private static Dictionary<string,double> Position(IDiscoveryVector vector, IDiscoveryClusterMapping mapping)
        {
            var source = new Dictionary<string,double>(StringComparer.Ordinal);
            for (var i=0;i<vector.Components.Count;i++) source[vector.Components[i].Name]=vector.Components[i].Value;
            var result = new Dictionary<string,double>(StringComparer.Ordinal);
            foreach(var dimension in mapping.Centroid.Keys)
            { if(!source.TryGetValue(dimension,out var value)) throw new InvalidOperationException("Vector is missing mapped dimension: "+dimension); result[dimension]=value; }
            return result;
        }
        private static Dictionary<string,double> Velocity(IReadOnlyList<IDiscoveryVectorMotion> motion, IDiscoveryClusterMapping mapping)
        {
            var source=new Dictionary<string,double>(StringComparer.Ordinal);
            for(var i=0;i<motion.Count;i++) source[motion[i].Dimension]=motion[i].Velocity;
            var result=new Dictionary<string,double>(StringComparer.Ordinal);
            foreach(var dimension in mapping.Centroid.Keys)
            { if(!source.TryGetValue(dimension,out var value)) throw new InvalidOperationException("Motion is missing mapped dimension: "+dimension); result[dimension]=value; }
            return result;
        }
        private static Dictionary<string,double> Delta(IReadOnlyDictionary<string,double> a,IReadOnlyDictionary<string,double> b)
        { var r=new Dictionary<string,double>(StringComparer.Ordinal);foreach(var p in b) r[p.Key]=a[p.Key]-p.Value;return r; }
        private static double Dot(IReadOnlyDictionary<string,double> a,IReadOnlyDictionary<string,double> b)
        { var t=0.0;foreach(var p in a)t+=p.Value*b[p.Key];return t; }
        private static double Magnitude(IReadOnlyDictionary<string,double> v)=>Math.Sqrt(Dot(v,v));
        private static Dictionary<string,double> At(IReadOnlyDictionary<string,double> p,IReadOnlyDictionary<string,double> v,double t)
        { var r=new Dictionary<string,double>(StringComparer.Ordinal);foreach(var x in p)r[x.Key]=x.Value+(v[x.Key]*t);return r; }
        private static double FirstAxis(string key,IReadOnlyDictionary<string,double> centroid)
        { foreach(var p in centroid)return string.Equals(p.Key,key,StringComparison.Ordinal)?1.0:0.0;return 0.0; }
    }
}
