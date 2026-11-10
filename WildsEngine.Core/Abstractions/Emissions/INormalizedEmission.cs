namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one resolved emission together with its
    /// normalized contribution to an emission vector.
    /// </summary>
    public interface INormalizedEmission
    {
        IResolvedEmission Emission { get; }

        double ResolvedValue { get; }

        double NormalizedValue { get; }
    }
}