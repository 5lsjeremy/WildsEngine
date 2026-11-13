using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    public abstract class FactBase :
        IFact
    {
        private readonly List<IDiscoveryEvidence> _evidence =
            new List<IDiscoveryEvidence>();

        private readonly Resolution _resolution;

        protected FactBase(
            IEntity entity,
            string name,
            object? value,
            int requiredEvidenceCount,
            IEvidenceContributionPolicy contributionPolicy,
            string? externalId = null)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            if (string.IsNullOrWhiteSpace(
                    name))
            {
                throw new ArgumentException(
                    "A Fact requires a name.",
                    nameof(name));
            }

            ContributionPolicy =
                contributionPolicy ??
                throw new ArgumentNullException(
                    nameof(contributionPolicy));

            Name =
                name.Trim();

            Value =
                value;

            ExternalId =
                externalId;

            _resolution =
                new Resolution(
                    _evidence,
                    requiredEvidenceCount,
                    ContributionPolicy);
        }

        public string? ExternalId { get; }

        public string Name { get; }

        public object? Value { get; private set; }

        public IEntity Entity { get; }

        public IEvidenceContributionPolicy ContributionPolicy { get; }

        public IReadOnlyList<IDiscoveryEvidence> Evidence =>
            _evidence;

        public IReadOnlyList<IDiscoveryEvidence> Contributors =>
            _resolution.Contributors;

        public int RequiredEvidenceCount =>
            _resolution.RequiredCount;

        public int CurrentEvidenceCount =>
            _resolution.CurrentCount;

        public double EvidencePercentage =>
            _resolution.Percentage;

        public bool IsProven =>
            _resolution.IsResolved;

        public int RequiredCount =>
            RequiredEvidenceCount;

        public int CurrentCount =>
            CurrentEvidenceCount;

        public double Percentage =>
            EvidencePercentage;

        public bool IsResolved =>
            IsProven;

        protected void SetValue(
            object? value)
        {
            Value =
                value;
        }

        protected bool AddEvidence(
            IDiscoveryEvidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            if (!ReferenceEquals(
                    evidence.Entity,
                    Entity))
            {
                throw new ArgumentException(
                    "Evidence must belong to this exact Entity.",
                    nameof(evidence));
            }

            if (_evidence.Contains(
                    evidence))
            {
                return false;
            }

            _evidence.Add(
                evidence);

            return true;
        }

        protected bool RemoveEvidence(
            IDiscoveryEvidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            return _evidence.Remove(
                evidence);
        }

        private sealed class Resolution :
            ResolutionThresholdBase<IDiscoveryEvidence>
        {
            private readonly IReadOnlyList<IDiscoveryEvidence> _items;

            private readonly IEvidenceContributionPolicy _policy;

            public Resolution(
                IReadOnlyList<IDiscoveryEvidence> items,
                int requiredCount,
                IEvidenceContributionPolicy policy)
                : base(
                    requiredCount)
            {
                _items =
                    items ??
                    throw new ArgumentNullException(
                        nameof(items));

                _policy =
                    policy ??
                    throw new ArgumentNullException(
                        nameof(policy));
            }

            protected override IReadOnlyList<IDiscoveryEvidence> Items =>
                _items;

            protected override bool IsValidContributor(
                IDiscoveryEvidence contributor)
            {
                if (contributor == null ||
                    !contributor.IsValidated)
                {
                    return false;
                }

                if (!ReferenceEquals(
                        contributor.ProofResult.Evidence,
                        contributor))
                {
                    return false;
                }

                return _policy.Contributes(
                    contributor);
            }
        }
    }
}