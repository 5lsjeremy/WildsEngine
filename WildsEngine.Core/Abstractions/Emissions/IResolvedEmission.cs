namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one immutable emission resolved by a Wilds
    /// emitter.
    ///
    /// The emission name remains consumer-owned vocabulary.
    /// The value is the authoritative scalar produced by
    /// emission resolution.
    /// </summary>
    public interface IResolvedEmission
    {
        IEmitter Source { get; }

        string Name { get; }

        double Value { get; }
    }
}