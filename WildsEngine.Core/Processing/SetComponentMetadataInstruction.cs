using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Processing
{
    public sealed class SetComponentMetadataInstruction :
        StateInstruction
    {
        public const string IntentName =
            "component.metadata.set";

        public SetComponentMetadataInstruction(
            string key,
            object? value,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                value,
                CreateArguments(key),
                metadata)
        {
            Key = ValidateKey(key);
            Value = value;
        }

        public string Key { get; }

        public object? Value { get; }

        private static IDictionary<string, object?> CreateArguments(
            string key)
        {
            return new Dictionary<string, object?>
            {
                ["key"] = ValidateKey(key)
            };
        }

        private static string ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Component metadata instructions require a key.",
                    nameof(key));
            }

            return key.Trim();
        }
    }
}

