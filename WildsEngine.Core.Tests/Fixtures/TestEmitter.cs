using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;

namespace WildsEngine.Core.Tests.Fixtures
{
    internal sealed class TestEmitter :
        EmitterBase
    {
        public TestEmitter(
            IComponent component,
            string layer,
            IEmissionDataSource dataSource,
            IEmissionScalar scalar)
            : base(
                component,
                layer,
                dataSource,
                scalar)
        {
        }

        public double ResolveTestTopologyScalar<TResult>(
            INeighborSequenceOperator topology,
            INeighborScalarOperator<TResult> scalar,
            Func<TResult, double> conversion)
        {
            return ResolveTopologyScalar(
                topology,
                scalar,
                conversion);
        }

        public double ComposeTestEmission(
            double emission,
            double topology,
            IEmissionComposition composition)
        {
            return ComposeEmission(
                emission,
                topology,
                composition);
        }

        public bool TryResolveTestEmission<TResult>(
            string propertyName,
            INeighborSequenceOperator topology,
            INeighborScalarOperator<TResult> topologyScalar,
            Func<TResult, double> conversion,
            IEmissionComposition composition,
            out double value)
        {
            return TryResolveEmission(
                propertyName,
                topology,
                topologyScalar,
                conversion,
                composition,
                out value);
        }

        public bool TryResolveTestEmissionResult<TResult>(
            string emissionName,
            INeighborSequenceOperator topology,
            INeighborScalarOperator<TResult> topologyScalar,
            Func<TResult, double> conversion,
            IEmissionComposition composition,
            out IResolvedEmission? result)
        {
            return TryResolveEmissionResult(
                emissionName,
                topology,
                topologyScalar,
                conversion,
                composition,
                out result);
        }
    }
}