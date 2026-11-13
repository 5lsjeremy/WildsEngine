namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents the controlled processing boundary for
    /// one Entity and one exact consumer-owned runtime object.
    /// </summary>
    public interface IEntitySurface
    {
        IEntity Entity { get; }

        object Target { get; }

        string Alias { get; }

        string ConsumerType { get; }
    }
}