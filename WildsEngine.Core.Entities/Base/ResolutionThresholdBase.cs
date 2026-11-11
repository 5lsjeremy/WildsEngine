using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class ResolutionThresholdBase<TContributor>
    {
        protected ResolutionThresholdBase(
            int requiredCount)
        {
            if (requiredCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCount),
                    "Resolution requires at least one contributor.");
            }

            RequiredCount =
                requiredCount;
        }

        protected abstract IReadOnlyList<TContributor> Items { get; }

        public IReadOnlyList<TContributor> Contributors =>
            Items;

        public int RequiredCount { get; }

        public int CurrentCount
        {
            get
            {
                var count = 0;

                for (var index = 0;
                     index < Items.Count;
                     index++)
                {
                    if (IsValidContributor(Items[index]))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public double Percentage =>
            Math.Min(
                1.0,
                (double)CurrentCount / RequiredCount);

        public bool IsResolved =>
            CurrentCount >= RequiredCount;

        protected abstract bool IsValidContributor(
            TContributor contributor);
    }
}
