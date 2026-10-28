using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Tests
{
    public sealed class ConsumerSetVariableInstruction :
        ContextVariableInstructionBase
    {
        public const string IntentName =
            "consumer.context.variable.set";

        public ConsumerSetVariableInstruction(
            string key,
            object? value)
            : base(
                IntentName,
                key,
                value)
        {
        }
    }
}