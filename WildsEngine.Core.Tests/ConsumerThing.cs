namespace WildsEngine.Core.Tests;

public sealed class ConsumerThing
{
    public ConsumerThing(
        string name,
        int value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public int Value { get; }
}