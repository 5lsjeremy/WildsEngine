using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Base implementation for identifiable Wilds objects
    /// capable of resolving consumer-owned data into scalar
    /// emissions.
    ///
    /// The emitter owns no consumer vocabulary. Property
    /// names, group names, and their values remain defined
    /// by the consumer.
    ///
    /// As a BaseObject, every emitter also participates in
    /// ordinary Wilds neighbor topology.
    /// </summary>
    public abstract class EmitterBase :
        BaseObject,
        IEmitter
    {
        private readonly IComponent _component;
        private readonly IEmissionScalar _scalar;

        protected EmitterBase(
            IComponent component,
            string layer,
            IEmissionDataSource dataSource,
            IEmissionScalar scalar)
            : base(layer)
        {
            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));

            DataSource =
                dataSource ??
                throw new ArgumentNullException(
                    nameof(dataSource));

            _scalar =
                scalar ??
                throw new ArgumentNullException(
                    nameof(scalar));
        }

        public override IComponent Component =>
            _component;

        public IEmissionDataSource DataSource { get; }

        protected IEmissionScalar Scalar =>
            _scalar;

        public virtual bool TryEmit(
            string propertyName,
            out double value)
        {
            value =
                0.0;

            if (string.IsNullOrWhiteSpace(
                    propertyName))
            {
                return false;
            }

            if (!DataSource.TryGetProperty(
                    propertyName,
                    out var rawValue))
            {
                return false;
            }

            value =
                ResolveScalar(
                    rawValue);

            return true;
        }

        public virtual bool TryEmitGroup(
            string groupName,
            out IReadOnlyDictionary<string, double> values)
        {
            values =
                new Dictionary<string, double>();

            if (string.IsNullOrWhiteSpace(
                    groupName))
            {
                return false;
            }

            if (!DataSource.TryGetGroup(
                    groupName,
                    out var rawValues))
            {
                return false;
            }

            var resolved =
                new Dictionary<string, double>(
                    StringComparer.Ordinal);

            foreach (var pair in rawValues)
            {
                if (string.IsNullOrWhiteSpace(
                        pair.Key))
                {
                    throw new InvalidOperationException(
                        "Emission groups cannot contain " +
                        "an empty property name.");
                }

                resolved.Add(
                    pair.Key,
                    ResolveScalar(
                        pair.Value));
            }

            values =
                resolved;

            return true;
        }

        protected virtual double ResolveScalar(
            object? value)
        {
            return _scalar.Resolve(
                value);
        }
        protected double ResolveTopologyScalar<TResult>(
            INeighborSequenceOperator topology,
            INeighborScalarOperator<TResult> scalar,
            System.Func<TResult, double> conversion)
        {
            if (topology == null)
            {
                throw new ArgumentNullException(
                    nameof(topology));
            }

            if (scalar == null)
            {
                throw new ArgumentNullException(
                    nameof(scalar));
            }

            if (conversion == null)
            {
                throw new ArgumentNullException(
                    nameof(conversion));
            }

            var relationships =
                ApplyNeighborOperator(
                    topology);

            var result =
                scalar.Apply(
                    this,
                    relationships);

            var value =
                conversion(
                    result);

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArithmeticException(
                    "Topology scalar must be finite.");
            }

            return value;
        }
        
        protected double ComposeEmission(
            double emission,
            double topology,
            IEmissionComposition composition)
        {
            if (composition == null)
            {
                throw new ArgumentNullException(
                    nameof(composition));
            }

            return composition.Compose(
                emission,
                topology);
        }
        
        protected bool TryResolveEmission<TResult>(
            string propertyName,
            INeighborSequenceOperator topology,
            INeighborScalarOperator<TResult> topologyScalar,
            Func<TResult, double> conversion,
            IEmissionComposition composition,
            out double value)
        {
            value =
                0.0;

            if (topology == null)
            {
                throw new ArgumentNullException(
                    nameof(topology));
            }

            if (topologyScalar == null)
            {
                throw new ArgumentNullException(
                    nameof(topologyScalar));
            }

            if (conversion == null)
            {
                throw new ArgumentNullException(
                    nameof(conversion));
            }

            if (composition == null)
            {
                throw new ArgumentNullException(
                    nameof(composition));
            }

            if (!TryEmit(
                    propertyName,
                    out var emission))
            {
                return false;
            }

            var resolvedTopology =
                ResolveTopologyScalar(
                    topology,
                    topologyScalar,
                    conversion);

            value =
                ComposeEmission(
                    emission,
                    resolvedTopology,
                    composition);

            return true;
        }
    }
}