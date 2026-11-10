namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Normalizes one resolved-emission vector while
    /// preserving its dimensions, exact emission references,
    /// and component ordering.
    /// </summary>
    public interface IEmissionNormalizationPolicy
    {
        INormalizedEmissionVector Normalize(
            IResolvedEmissionVector vector);
    }
}