using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public sealed class SetContextVariable :
        TaskBase
    {
        public SetContextVariable(
            IComponent component,
            IContext context,
            string key,
            object? value)
            : base(
                component,
                "task.context.variable.set")
        {
            Context = context ??
                      throw new ArgumentNullException(
                          nameof(context));

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Context variable key cannot be empty.",
                    nameof(key));
            }

            Key = key.Trim();
            Value = value;
        }

        public IContext Context { get; }

        public string Key { get; }

        public object? Value { get; }
    }
}