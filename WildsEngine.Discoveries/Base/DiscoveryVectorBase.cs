using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable immutable Discovery-vector evaluation.
    ///
    /// Magnitude is calculated by the supplied metric from the
    /// exact validated component snapshot.
    /// </summary>
    public abstract class DiscoveryVectorBase :
        IDiscoveryVector
    {
        protected DiscoveryVectorBase(
            IReadOnlyList<IDiscoveryVectorComponent> components,
            IReadOnlyList<IDiscoveryVectorContribution> contributions,
            IReadOnlyList<IDiscoveryVectorMotion> motion,
            IDiscoveryVectorMetric metric,
            DateTimeOffset evaluatedAt)
        {
            Components =
                DiscoverySnapshots.Required(
                    components,
                    nameof(components));

            Contributions =
                DiscoverySnapshots.Required(
                    contributions,
                    nameof(contributions));

            Motion =
                DiscoverySnapshots.Required(
                    motion,
                    nameof(motion));

            DiscoveryDimensions.ValidateComponents(
                Components,
                nameof(components));

            DiscoveryDimensions.ValidateContributions(
                Contributions,
                Components,
                nameof(contributions));

            DiscoveryDimensions.ValidateMotion(
                Motion,
                Components,
                nameof(motion));

            Metric =
                metric ??
                throw new ArgumentNullException(
                    nameof(metric));

            EvaluatedAt =
                evaluatedAt;

            var magnitude =
                Metric.GetMagnitude(
                    this);

            Magnitude =
                DiscoveryNumbers.NonNegative(
                    magnitude,
                    nameof(magnitude));
        }

        public IReadOnlyList<IDiscoveryVectorComponent> Components { get; }

        public int Dimension =>
            Components.Count;

        public IReadOnlyList<IDiscoveryVectorContribution> Contributions
        {
            get;
        }

        public IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }

        public IDiscoveryVectorMetric Metric { get; }

        public double Magnitude { get; }

        public DateTimeOffset EvaluatedAt { get; }
    }
}
