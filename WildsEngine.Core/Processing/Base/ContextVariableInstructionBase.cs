using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public abstract class ContextVariableInstructionBase :
        StateInstructionBase,
        ISetContextVariableInstruction
    {
        protected ContextVariableInstructionBase(
            string intent,
            string key,
            object? value)
            : base(
                intent,
                value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Context variable key cannot be empty.",
                    nameof(key));
            }

            Key = key.Trim();
            Value = value;

            SetArgument(
                "key",
                Key);

            SetArgument(
                "value",
                Value);
        }

        public string Key { get; }

        public object? Value { get; }
    }
}