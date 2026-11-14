namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one consumer-authorized contribution from an
    /// authoritative Discovery input into a named vector
    /// dimension.
    /// </summary>
    public interface IDiscoveryVectorContribution
    {
        IDiscoveryVectorInput Input { get; }

        IFact? Fact { get; }

        string Dimension { get; }

        double Value { get; }

        double Weight { get; }
    }
}
