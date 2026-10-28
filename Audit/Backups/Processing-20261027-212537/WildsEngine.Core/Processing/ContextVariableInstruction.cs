using System;

namespace WildsEngine.Core.Processing
{
    public abstract class ContextVariableInstruction :
        StateInstruction
    {
        protected ContextVariableInstruction(
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

            Key = key;
            Value = value;

            SetArgument("key", key);
            SetArgument("value", value);
        }

        public string Key { get; }

        public object? Value { get; }
    }
}