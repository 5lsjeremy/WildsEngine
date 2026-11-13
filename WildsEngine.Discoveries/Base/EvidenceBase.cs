using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Represents one consumer-facing observation gathered
    /// from the world.
    ///
    /// Evidence records what occurred.
    /// It does not decide which Fact it proves or which
    /// Discovery may eventually consume that Fact.
    /// </summary>
    public abstract class EvidenceBase :
        IDiscoveryEvidence
    {
        private IProofResult? _proofResult;

        protected EvidenceBase(
            IEntity entity,
            IReadOnlyDictionary<string, object?> conditions,
            object? outcome,
            double strength,
            double normalizedStrength,
            IReadOnlyList<IBaseObject> neighbors,
            IBaseUnit? source,
            string? externalId = null,
            string? name = null)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            if (conditions == null)
            {
                throw new ArgumentNullException(
                    nameof(conditions));
            }

            if (neighbors == null)
            {
                throw new ArgumentNullException(
                    nameof(neighbors));
            }

            if (double.IsNaN(
                    strength) ||
                double.IsInfinity(
                    strength))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(strength),
                    "Evidence strength must be finite.");
            }

            if (double.IsNaN(
                    normalizedStrength) ||
                double.IsInfinity(
                    normalizedStrength) ||
                normalizedStrength < 0.0 ||
                normalizedStrength > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedStrength),
                    "Normalized evidence strength must be " +
                    "between zero and one.");
            }

            Conditions =
                new ReadOnlyDictionary<string, object?>(
                    new Dictionary<string, object?>(
                        conditions,
                        StringComparer.Ordinal));

            Neighbors =
                new ReadOnlyCollection<IBaseObject>(
                    new List<IBaseObject>(
                        neighbors));

            Outcome =
                outcome;

            Strength =
                strength;

            NormalizedStrength =
                normalizedStrength;

            Source =
                source;

            ExternalId =
                externalId;

            Name =
                name;
        }

        public string? ExternalId { get; }

        public string? Name { get; }

        public IEntity Entity { get; }

        public IReadOnlyDictionary<string, object?>
            Conditions { get; }

        public object? Outcome { get; }

        public double Strength { get; }

        public double NormalizedStrength { get; }

        public IReadOnlyList<IBaseObject> Neighbors { get; }

        public IBaseUnit? Source { get; }

        public IProofResult ProofResult =>
            _proofResult ??
            throw new InvalidOperationException(
                "Evidence has not been validated.");

        public bool IsValidated =>
            _proofResult != null;

        protected bool SetProofResult(
            IProofResult proofResult)
        {
            if (proofResult == null)
            {
                throw new ArgumentNullException(
                    nameof(proofResult));
            }

            if (!ReferenceEquals(
                    proofResult.Evidence,
                    this))
            {
                throw new ArgumentException(
                    "The ProofResult must belong to this " +
                    "exact Evidence instance.",
                    nameof(proofResult));
            }

            if (_proofResult != null)
            {
                return false;
            }

            _proofResult =
                proofResult;

            return true;
        }
    }
}