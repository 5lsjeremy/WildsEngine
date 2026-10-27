using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public sealed class StateInstruction : IStateInstruction
    {
        private readonly IReadOnlyDictionary<string, object> _metadata;

        public StateInstruction(
            string operation,
            string target,
            object value)
            : this(
                operation,
                target,
                value,
                new Dictionary<string, object>())
        {
        }

        public StateInstruction(
            string operation,
            string target,
            object value,
            IReadOnlyDictionary<string, object> metadata)
        {
            if (string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException(
                    "State instructions require an operation.",
                    nameof(operation));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException(
                    "State instructions require a target.",
                    nameof(target));

            Operation = operation.Trim();
            Target = target.Trim();
            Value = value;
            _metadata = metadata ??
                throw new ArgumentNullException(nameof(metadata));
        }

        public string Operation { get; }

        public string Target { get; }

        public object Value { get; }

        public IReadOnlyDictionary<string, object> Metadata =>
            _metadata;
    }
}
