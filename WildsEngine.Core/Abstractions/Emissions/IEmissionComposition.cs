namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Combines a base emission scalar with a scalar contribution
    /// derived from the emitter's current topology.
    ///
    /// The composition rule owns the meaning of that combination.
    /// </summary>
    public interface IEmissionComposition
    {
        double Compose(
            double emission,
            double topology);
    }
}