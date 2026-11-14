using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable base for scoped Truths congealed from Discovery
    /// composition.
    ///
    /// Truth reuses established Discoveries identity and retains
    /// the exact scoped vector series whose composition
    /// congealed.
    /// </summary>
    public abstract class TruthBase :
        IdentifierBase,
        ITruth
    {
        protected TruthBase(
            string slug,
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
            : base(
                slug)
        {
            Initialize(
                name,
                value,
                series,
                clusters,
                discoveries,
                stability,
                confidence,
                congealedAt);
        }

        protected TruthBase(
            Guid id,
            string slug,
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
            : base(
                id,
                slug)
        {
            Initialize(
                name,
                value,
                series,
                clusters,
                discoveries,
                stability,
                confidence,
                congealedAt);
        }

        public string Name { get; private set; } = null!;

        public object? Value { get; private set; }

        public IDiscoveryVectorSeries Series
        {
            get;
            private set;
        } = null!;

        public IReadOnlyList<IDiscoveryCluster> Clusters
        {
            get;
            private set;
        } = null!;

        public IReadOnlyList<IDiscovery> Discoveries
        {
            get;
            private set;
        } = null!;

        public double Stability { get; private set; }

        public double Confidence { get; private set; }

        public DateTimeOffset CongealedAt { get; private set; }

        private void Initialize(
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Truth name cannot be empty.",
                    nameof(name));
            }

            Name =
                name.Trim();

            Value =
                value;

            Series =
                series ??
                throw new ArgumentNullException(
                    nameof(series));

            if (string.IsNullOrWhiteSpace(
                    Series.Scope))
            {
                throw new ArgumentException(
                    "Truth requires a scoped Discovery-vector series.",
                    nameof(series));
            }

            if (Series.Inputs == null)
            {
                throw new ArgumentException(
                    "Truth requires a series with an input population.",
                    nameof(series));
            }

            if (Series.Vectors == null)
            {
                throw new ArgumentException(
                    "Truth requires a series with vector evaluations.",
                    nameof(series));
            }

            Clusters =
                DiscoverySnapshots.Required(
                    clusters,
                    nameof(clusters));

            Discoveries =
                DiscoverySnapshots.Required(
                    discoveries,
                    nameof(discoveries));

            ValidateDiscoveriesBelongToSeries(
                Discoveries,
                Series);

            Stability =
                DiscoveryNumbers.Unit(
                    stability,
                    nameof(stability));

            Confidence =
                DiscoveryNumbers.Unit(
                    confidence,
                    nameof(confidence));

            CongealedAt =
                congealedAt;
        }

        private static void ValidateDiscoveriesBelongToSeries(
            IReadOnlyList<IDiscovery> discoveries,
            IDiscoveryVectorSeries series)
        {
            for (var discoveryIndex = 0;
                 discoveryIndex < discoveries.Count;
                 discoveryIndex++)
            {
                var discovery =
                    discoveries[discoveryIndex];

                var found =
                    false;

                for (var inputIndex = 0;
                     inputIndex < series.Inputs.Count;
                     inputIndex++)
                {
                    var input =
                        series.Inputs[inputIndex];

                    if (input == null)
                    {
                        throw new ArgumentException(
                            "Truth series inputs cannot contain null items.",
                            nameof(series));
                    }

                    if (ReferenceEquals(
                            discovery,
                            input.Discovery))
                    {
                        found =
                            true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        "Every Truth Discovery must belong to the exact congealment series.",
                        nameof(discoveries));
                }
            }
        }
    }
}
