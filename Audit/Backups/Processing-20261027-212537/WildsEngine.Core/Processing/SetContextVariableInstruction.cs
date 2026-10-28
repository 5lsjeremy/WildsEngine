using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public sealed class SetContextVariableInstruction :
        StateInstruction,
        ISetContextVariableInstruction
    {
        public SetContextVariableInstruction(
            string key,
            object? value)
            : base(
                "context.variable.set",
                value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Variable key cannot be empty.",
                    nameof(key));
            }

            Key = key;
            Value = value;
        }

        public string Key { get; }

        public object? Value { get; }
    }
}