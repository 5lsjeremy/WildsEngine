namespace WildsEngine.Core.Processing
{
    public sealed class SetContextVariableInstruction :
        ContextVariableInstructionBase
    {
        public const string IntentName =
            "context.variable.set";

        public SetContextVariableInstruction(
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