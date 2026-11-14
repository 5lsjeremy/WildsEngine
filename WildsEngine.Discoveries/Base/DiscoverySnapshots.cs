using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Shared immutable snapshot mechanics for Discoveries
    /// composition results.
    /// </summary>
    internal static class DiscoverySnapshots
    {
        public static IReadOnlyList<T> Required<T>(
            IReadOnlyList<T> source,
            string parameterName)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            var snapshot =
                new T[source.Count];

            for (var index = 0;
                 index < source.Count;
                 index++)
            {
                snapshot[index] =
                    source[index] ??
                    throw new ArgumentException(
                        "Snapshot collections cannot contain null items.",
                        parameterName);
            }

            return new ReadOnlyCollection<T>(
                snapshot);
        }
    }
}
