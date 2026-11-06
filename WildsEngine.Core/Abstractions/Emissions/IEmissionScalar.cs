namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Converts one consumer-owned raw value into the scalar
    /// value used by Wilds emission processing.
    /// </summary>
    public interface IEmissionScalar
    {
        double Resolve(
            object? value);
    }
}