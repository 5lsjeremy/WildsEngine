namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one consumer-defined coordinate in Discovery
    /// geometry.
    ///
    /// Wilds owns the numeric mechanics. The consumer owns the
    /// meaning of Name.
    /// </summary>
    public interface IDiscoveryVectorComponent
    {
        string Name { get; }

        double Value { get; }
    }
}
