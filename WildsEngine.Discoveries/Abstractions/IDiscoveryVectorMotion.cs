namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents temporal movement along one consumer-defined
    /// Discovery dimension.
    ///
    /// Motion is separate from the coordinate value itself.
    /// </summary>
    public interface IDiscoveryVectorMotion
    {
        string Dimension { get; }

        double Velocity { get; }

        double Acceleration { get; }
    }
}
