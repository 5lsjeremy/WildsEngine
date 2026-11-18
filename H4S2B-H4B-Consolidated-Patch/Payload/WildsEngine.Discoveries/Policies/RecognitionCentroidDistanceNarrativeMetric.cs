using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class RecognitionCentroidDistanceNarrativeMetric : IdentifierBase, IDiscoveryNarrativeMetric
    {
        public RecognitionCentroidDistanceNarrativeMetric(string slug = "recognition-centroid-distance")
            : base(Guid.NewGuid(), slug) { }

        public double Measure(IDiscoveryClusterRecognitionResult left, IDiscoveryClusterRecognitionResult right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (left.Mapping.Centroid.Count != right.Mapping.Centroid.Count)
                throw new InvalidOperationException("Narrative mappings must share the exact same dimensions.");
            double sum = 0.0;
            foreach (var component in left.Mapping.Centroid)
            {
                if (!right.Mapping.Centroid.TryGetValue(component.Key, out var rightValue))
                    throw new InvalidOperationException("Narrative mappings must share the exact same dimensions.");
                var delta = component.Value - rightValue;
                var squared = delta * delta;
                if (double.IsNaN(squared) || double.IsInfinity(squared))
                    throw new InvalidOperationException("Narrative centroid distance overflowed.");
                sum += squared;
                if (double.IsNaN(sum) || double.IsInfinity(sum))
                    throw new InvalidOperationException("Narrative centroid distance overflowed.");
            }
            var result = Math.Sqrt(sum);
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new InvalidOperationException("Narrative metric returned a non-finite value.");
            return result;
        }
    }
}
