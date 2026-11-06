using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Provides consumer-owned values to Wilds emission processing
    /// without exposing the consumer's underlying storage model.
    ///
    /// Property names, group names, and values are consumer-owned
    /// vocabulary. The source retains ownership of the supplied data.
    /// </summary>
    public interface IEmissionDataSource
    {
        bool TryGetProperty(
            string propertyName,
            out object? value);

        bool TryGetGroup(
            string groupName,
            out IReadOnlyDictionary<string, object?> values);
    }
}