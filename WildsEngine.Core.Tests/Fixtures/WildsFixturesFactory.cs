using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;

namespace WildsEngine.Core.Tests.Fixtures
{
    internal static class WildsFixtureFactory
    {
        public static TestEmitter CreateEmitter(
            IComponent component,
            string layer = "test.emitter",
            IReadOnlyDictionary<
                string,
                object?>? properties = null,
            IReadOnlyDictionary<
                string,
                IReadOnlyDictionary<
                    string,
                    object?>>? groups = null,
            IEmissionScalar? scalar = null)
        {
            var dataSource =
                new TestEmissionDataSource(
                    properties,
                    groups);

            return new TestEmitter(
                component,
                layer,
                dataSource,
                scalar ??
                new EmissionScalar());
        }

        public static TestTopologyObject CreateTopologyObject(
            IComponent component,
            string layer = "test.topology")
        {
            return new TestTopologyObject(
                component,
                layer);
        }

        public static TestTopologyObject CreateCheese(
            IComponent component)
        {
            return CreateTopologyObject(
                component,
                "test.cheese");
        }
    }
}