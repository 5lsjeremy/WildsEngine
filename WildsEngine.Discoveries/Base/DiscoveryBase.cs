using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    public abstract class DiscoveryBase :
        IDiscovery
    {
        private readonly List<IFact> _facts =
            new List<IFact>();

        private readonly Resolution _resolution;

        protected DiscoveryBase(
            IEntity entity,
            int requiredFactCount,
            string? name = null,
            string? externalId = null)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            Name =
                name;

            ExternalId =
                externalId;

            _resolution =
                new Resolution(
                    _facts,
                    requiredFactCount);
        }

        public string? ExternalId { get; }

        public string? Name { get; }

        public IEntity Entity { get; }

        public IReadOnlyList<IFact> Facts =>
            _facts;

        public IReadOnlyList<IFact> Contributors =>
            _resolution.Contributors;

        public int RequiredFactCount =>
            _resolution.RequiredCount;

        public int CurrentFactCount =>
            _resolution.CurrentCount;

        public double FactPercentage =>
            _resolution.Percentage;

        public bool IsAvailable =>
            _resolution.IsResolved;

        public int RequiredCount =>
            RequiredFactCount;

        public int CurrentCount =>
            CurrentFactCount;

        public double Percentage =>
            FactPercentage;

        public bool IsResolved =>
            IsAvailable;

        protected bool AddFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            if (!ReferenceEquals(
                    fact.Entity,
                    Entity))
            {
                throw new ArgumentException(
                    "A Fact must belong to this exact Discovery Entity.",
                    nameof(fact));
            }

            if (_facts.Contains(
                    fact))
            {
                return false;
            }

            _facts.Add(
                fact);

            return true;
        }

        protected bool RemoveFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            return _facts.Remove(
                fact);
        }

        private sealed class Resolution :
            ResolutionThresholdBase<IFact>
        {
            private readonly IReadOnlyList<IFact> _items;

            public Resolution(
                IReadOnlyList<IFact> items,
                int requiredCount)
                : base(
                    requiredCount)
            {
                _items =
                    items ??
                    throw new ArgumentNullException(
                        nameof(items));
            }

            protected override IReadOnlyList<IFact> Items =>
                _items;

            protected override bool IsValidContributor(
                IFact contributor)
            {
                return contributor != null &&
                       contributor.IsProven;
            }
        }
    }
}