using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public abstract class StateInstructionBase :
        IStateInstruction
    {
        private readonly Dictionary<string, object?> _arguments;
        private readonly Dictionary<string, object?> _metadata;

        protected StateInstructionBase(
            string intent,
            object? payload = null,
            IEnumerable<KeyValuePair<string, object?>>? arguments = null,
            IEnumerable<KeyValuePair<string, object?>>? metadata = null)
        {
            if (string.IsNullOrWhiteSpace(intent))
            {
                throw new ArgumentException(
                    "Instruction intent cannot be empty.",
                    nameof(intent));
            }

            Id = Guid.NewGuid();
            Intent = intent;
            Payload = payload;

            _arguments = CreateDictionary(arguments);
            _metadata = CreateDictionary(metadata);
        }

        public Guid Id { get; }

        public string Intent { get; }

        public object? Payload { get; }

        public IReadOnlyDictionary<string, object?> Arguments =>
            _arguments;

        public IReadOnlyDictionary<string, object?> Metadata =>
            _metadata;

        protected void SetArgument(
            string key,
            object? value)
        {
            ValidateKey(key, nameof(key));

            _arguments[key] = value;
        }

        protected void SetMetadata(
            string key,
            object? value)
        {
            ValidateKey(key, nameof(key));

            _metadata[key] = value;
        }

        protected bool RemoveArgument(string key)
        {
            ValidateKey(key, nameof(key));

            return _arguments.Remove(key);
        }

        protected bool RemoveMetadata(string key)
        {
            ValidateKey(key, nameof(key));

            return _metadata.Remove(key);
        }

        private static Dictionary<string, object?> CreateDictionary(
            IEnumerable<KeyValuePair<string, object?>>? source)
        {
            var result =
                new Dictionary<string, object?>();

            if (source == null)
            {
                return result;
            }

            foreach (var pair in source)
            {
                ValidateKey(pair.Key, nameof(source));

                result[pair.Key] = pair.Value;
            }

            return result;
        }

        private static void ValidateKey(
            string key,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Instruction keys cannot be empty.",
                    parameterName);
            }
        }
    }
}