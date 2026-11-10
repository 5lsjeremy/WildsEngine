using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable carrier for one resolved Wilds emission.
    ///
    /// The carrier preserves the exact emitter, consumer-owned
    /// emission identity, and authoritative resolved scalar.
    ///
    /// It owns no propagation, eligibility, or receiver behavior.
    /// </summary>
    public sealed class ResolvedEmission :
        IResolvedEmission
    {
        public ResolvedEmission(
            IEmitter source,
            string name,
            double value)
        {
            Source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            if (string.IsNullOrWhiteSpace(
                    name))
            {
                throw new ArgumentException(
                    "A resolved emission requires a name.",
                    nameof(name));
            }

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "A resolved emission value must be finite.");
            }

            Name =
                name.Trim();

            Value =
                value;
        }

        public IEmitter Source { get; }

        public string Name { get; }

        public double Value { get; }
    }
}