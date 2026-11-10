using System;

namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Represents stable identity established outside
    /// the Wilds processing boundary.
    ///
    /// Identity does not imply Wilds registration,
    /// promotion, topology participation, or processing.
    /// </summary>
    public interface IIdentifier
    {
        Guid Id { get; }

        string Slug { get; }
    }
}