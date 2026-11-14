namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Calculates magnitude for one Discovery-vector point.
    ///
    /// The metric supplies the geometry. A vector cannot accept
    /// an unrelated caller-supplied magnitude.
    /// </summary>
    public interface IDiscoveryVectorMetric
    {
        double GetMagnitude(
            IDiscoveryVectorPoint point);
    }
}
