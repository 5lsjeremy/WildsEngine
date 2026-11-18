using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeCompositionDecision :
        IDiscoveryNarrativeCompositionDecision
    {
        public DiscoveryNarrativeCompositionDecision(
            IDiscoveryClusterRecognitionResult left,
            IDiscoveryClusterRecognitionResult right,
            double measurement,
            IDiscoveryNarrativeRelationshipDefinition? relationship,
            bool canCompose)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            if (ReferenceEquals(left, right))
                throw new ArgumentException("A composition decision requires two distinct recognition results.");
            if (double.IsNaN(measurement) || double.IsInfinity(measurement))
                throw new ArgumentOutOfRangeException(nameof(measurement));
            Measurement = measurement;
            Relationship = relationship;
            CanCompose = canCompose;
        }

        public IDiscoveryClusterRecognitionResult Left { get; }
        public IDiscoveryClusterRecognitionResult Right { get; }
        public double Measurement { get; }
        public IDiscoveryNarrativeRelationshipDefinition? Relationship { get; }
        public bool CanCompose { get; }
    }
}
