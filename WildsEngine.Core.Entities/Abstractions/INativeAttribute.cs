using System;

namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Describes one native consumer attribute observed when an Entity surface
    /// is created. The captured value is historical and is not authoritative
    /// for the consumer's current live value.
    /// </summary>
    public interface INativeAttribute
    {
        string Name { get; }

        Type ValueType { get; }

        object? InitialValue { get; }

        bool CanRead { get; }

        bool CanWrite { get; }
    }
}
