using WildsEngine.Core.Processing;

public sealed class ConsumerSetVariableInstruction :
    ContextVariableInstruction
{
    public ConsumerSetVariableInstruction(
        string key,
        object? value)
        : base(
            "consumer.context.variable.set",
            key,
            value)
    {
    }
}