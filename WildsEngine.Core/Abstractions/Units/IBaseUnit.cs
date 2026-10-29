using System;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Defines the common identity contract shared by
    /// Wilds temporal and processing units.
    /// </summary>
    public interface IBaseUnit
    {
        Guid Id { get; }
    }
}
