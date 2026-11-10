namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one resolved emission after propagation
    /// through one exact Neighbor relationship.
    ///
    /// ResolvedValue preserves the authoritative source
    /// magnitude.
    ///
    /// NormalizedValue identifies the bounded input used for
    /// vector propagation. It is null when the emission was
    /// propagated directly without vector normalization.
    ///
    /// Value represents the intensity received through the
    /// relationship after propagation.
    /// </summary>
    public interface IPropagatedEmission
    {
        IResolvedEmission Emission { get; }

        INeighborRelationship Relationship { get; }

        double PropagationDistance { get; }

        double ResolvedValue { get; }

        double? NormalizedValue { get; }

        double Value { get; }
    }
}