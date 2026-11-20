using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Surface
{
    /// <summary>
    /// Provides explicit, repeatable observation-history keys without
    /// introducing a second ownership or identity model.
    /// </summary>
    public static class DiscoveryHistoryKey
    {
        public static string ForEntity(IEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Slug;
        }

        public static string ForSubject(string subjectId)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("A subject ID is required.", nameof(subjectId));

            return subjectId.Trim();
        }

        public static string ForPopulation(string populationId)
        {
            if (string.IsNullOrWhiteSpace(populationId))
                throw new ArgumentException("A population ID is required.", nameof(populationId));

            return populationId.Trim();
        }

        public static string ForPerspective(string observerId, string subjectId)
        {
            if (string.IsNullOrWhiteSpace(observerId))
                throw new ArgumentException("An observer ID is required.", nameof(observerId));
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("A subject ID is required.", nameof(subjectId));

            return observerId.Trim() + ":" + subjectId.Trim();
        }
    }
}
