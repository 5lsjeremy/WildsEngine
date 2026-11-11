using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Concrete
{
    public sealed class NativeAttribute :
        INativeAttribute
    {
        public NativeAttribute(
            string name,
            Type valueType,
            object? initialValue,
            bool canRead,
            bool canWrite)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "A native attribute requires a name.",
                    nameof(name));
            }

            Name =
                name.Trim();

            ValueType =
                valueType ??
                throw new ArgumentNullException(
                    nameof(valueType));

            InitialValue =
                initialValue;

            CanRead =
                canRead;

            CanWrite =
                canWrite;
        }

        public string Name { get; }
        public Type ValueType { get; }
        public object? InitialValue { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
    }
}
