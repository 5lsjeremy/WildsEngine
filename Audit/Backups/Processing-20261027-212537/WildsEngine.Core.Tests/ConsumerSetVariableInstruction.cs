using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Tests
{
    public sealed class ConsumerSetVariableInstruction :
        StateInstruction,
        ISetContextVariableInstruction
    {
        public ConsumerSetVariableInstruction(
            string key,
            object? value)
            : base(
                "consumer.context.variable.set",
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