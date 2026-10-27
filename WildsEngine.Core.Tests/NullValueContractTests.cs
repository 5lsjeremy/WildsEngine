using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using Xunit;

namespace WildsEngine.Core.Tests
{
    public sealed class NullValueContractTests
    {
        [Fact]
        public void ContextVariableDictionary_CanRepresentNull()
        {
            var values = new Dictionary<string, object?>
            {
                ["target"] = null
            };

            IReadOnlyDictionary<string, object?> readOnly = values;

            Assert.True(readOnly.ContainsKey("target"));
            Assert.Null(readOnly["target"]);
        }

        [Fact]
        public void NullValue_IsDistinctFromMissingKey()
        {
            var values = new Dictionary<string, object?>
            {
                ["target"] = null
            };

            IReadOnlyDictionary<string, object?> readOnly = values;

            Assert.True(readOnly.ContainsKey("target"));
            Assert.False(readOnly.ContainsKey("missing"));
        }

        [Fact]
        public void ValueCanTransitionToNull()
        {
            var expected = new ConsumerThing("external");

            var values = new Dictionary<string, object?>
            {
                ["target"] = expected
            };

            values["target"] = null;

            Assert.True(values.ContainsKey("target"));
            Assert.Null(values["target"]);
        }

        [Fact]
        public void NullCanTransitionToConsumerObject()
        {
            var expected = new ConsumerThing("external");

            var values = new Dictionary<string, object?>
            {
                ["target"] = null
            };

            values["target"] = expected;

            Assert.Same(expected, values["target"]);
        }

        [Fact]
        public void ArbitraryConsumerObjectRetainsConcreteType()
        {
            var expected = new ConsumerThing("external");

            var values = new Dictionary<string, object?>
            {
                ["target"] = expected
            };

            var actual = values["target"];

            Assert.Same(expected, actual);
            Assert.IsType<ConsumerThing>(actual);
        }

        private sealed class ConsumerThing
        {
            public ConsumerThing(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
