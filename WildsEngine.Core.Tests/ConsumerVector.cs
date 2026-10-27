namespace WildsEngine.Core.Tests;

public readonly struct ConsumerVector :
    IEquatable<ConsumerVector>
{
    public ConsumerVector(
        float x,
        float y,
        float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; }

    public float Y { get; }

    public float Z { get; }

    public bool Equals(ConsumerVector other)
    {
        return X.Equals(other.X)
               && Y.Equals(other.Y)
               && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConsumerVector other
               && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            hash = (hash * 31) + X.GetHashCode();
            hash = (hash * 31) + Y.GetHashCode();
            hash = (hash * 31) + Z.GetHashCode();

            return hash;
        }
    }
}