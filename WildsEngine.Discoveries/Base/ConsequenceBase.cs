using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable base for projected Consequences.
    ///
    /// Consequence uses the established Discoveries identity
    /// model and remains knowledge rather than behavior.
    /// </summary>
    public abstract class ConsequenceBase :
        IdentifierBase,
        IConsequence
    {
        protected ConsequenceBase(
            string slug,
            string name,
            object? value,
            IReadOnlyList<ITruth> truths,
            double magnitude,
            double confidence,
            DateTimeOffset projectedAt)
            : base(
                slug)
        {
            Initialize(
                name,
                value,
                truths,
                magnitude,
                confidence,
                projectedAt);
        }

        protected ConsequenceBase(
            Guid id,
            string slug,
            string name,
            object? value,
            IReadOnlyList<ITruth> truths,
            double magnitude,
            double confidence,
            DateTimeOffset projectedAt)
            : base(
                id,
                slug)
        {
            Initialize(
                name,
                value,
                truths,
                magnitude,
                confidence,
                projectedAt);
        }

        public string Name { get; private set; } = null!;

        public object? Value { get; private set; }

        public IReadOnlyList<ITruth> Truths
        {
            get;
            private set;
        } = null!;

        public double Magnitude { get; private set; }

        public double Confidence { get; private set; }

        public DateTimeOffset ProjectedAt { get; private set; }

        private void Initialize(
            string name,
            object? value,
            IReadOnlyList<ITruth> truths,
            double magnitude,
            double confidence,
            DateTimeOffset projectedAt)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Consequence name cannot be empty.",
                    nameof(name));
            }

            Name =
                name.Trim();

            Value =
                value;

            Truths =
                DiscoverySnapshots.Required(
                    truths,
                    nameof(truths));

            Magnitude =
                DiscoveryNumbers.NonNegative(
                    magnitude,
                    nameof(magnitude));

            Confidence =
                DiscoveryNumbers.Unit(
                    confidence,
                    nameof(confidence));

            ProjectedAt =
                projectedAt;
        }
    }
}
