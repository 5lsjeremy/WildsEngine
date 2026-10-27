using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Components;
using WildsEngine.Core.Objects;
using Xunit;

namespace WildsEngine.Core.Tests
{
    public sealed class ArbitraryObjectProcessingTests
    {
        [Fact]
        public void State_processes_external_reference_type_without_core_knowledge()
        {
            var component = new TestComponent("test");

            var payload = new ConsumerThing(
                "External Consumer Object",
                42);

            var instruction = new SetContextVariableInstruction(
                "consumer",
                payload);

            var processed = component.State.Process(instruction);

            Assert.True(processed);
            Assert.True(
                component.State.Context.Variables.ContainsKey("consumer"));

            var stored =
                component.State.Context.Variables["consumer"];

            Assert.Same(payload, stored);

            var typed = Assert.IsType<ConsumerThing>(stored);

            Assert.Equal("External Consumer Object", typed.Name);
            Assert.Equal(42, typed.Value);
        }

        [Fact]
        public void State_processes_external_value_type_without_core_knowledge()
        {
            var component = new TestComponent("test");

            var payload = new ConsumerVector(
                12.5f,
                -4.25f,
                81.75f);

            var instruction = new SetContextVariableInstruction(
                "position",
                payload);

            var processed = component.State.Process(instruction);

            Assert.True(processed);
            Assert.True(
                component.State.Context.Variables.ContainsKey("position"));

            var stored =
                component.State.Context.Variables["position"];

            var typed = Assert.IsType<ConsumerVector>(stored);

            Assert.Equal(payload, typed);
            Assert.Equal(12.5f, typed.X);
            Assert.Equal(-4.25f, typed.Y);
            Assert.Equal(81.75f, typed.Z);
        }

        [Fact]
        public void State_can_replace_an_existing_arbitrary_runtime_value()
        {
            var component = new TestComponent("test");

            var original = new ConsumerThing(
                "Original",
                1);

            var replacement = new ConsumerThing(
                "Replacement",
                2);

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        original)));

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        replacement)));

            var stored =
                component.State.Context.Variables["subject"];

            Assert.Same(replacement, stored);
            Assert.NotSame(original, stored);

            var typed = Assert.IsType<ConsumerThing>(stored);

            Assert.Equal("Replacement", typed.Name);
            Assert.Equal(2, typed.Value);
        }

        [Fact]
        public void State_processing_can_transition_existing_value_to_null()
        {
            var component = new TestComponent("test");

            var original = new ConsumerThing(
                "Original",
                1);

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        original)));

            Assert.Same(
                original,
                component.State.Context.Variables["subject"]);

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        null)));

            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "subject"));

            Assert.Null(
                component.State.Context.Variables["subject"]);
        }

        [Fact]
        public void State_processing_can_transition_null_to_consumer_object()
        {
            var component = new TestComponent("test");

            var replacement = new ConsumerThing(
                "Replacement",
                2);

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        null)));

            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "subject"));

            Assert.Null(
                component.State.Context.Variables["subject"]);

            Assert.True(
                component.State.Process(
                    new SetContextVariableInstruction(
                        "subject",
                        replacement)));

            var stored =
                component.State.Context.Variables["subject"];

            Assert.Same(replacement, stored);

            var typed = Assert.IsType<ConsumerThing>(stored);

            Assert.Equal("Replacement", typed.Name);
            Assert.Equal(2, typed.Value);
        }

        [Fact]
        public void Context_metadata_remains_separate_from_component_metadata()
        {
            var component = new TestComponent("test");

            component.Metadata["category"] = "component";
            component.State.Context.SetMetadata(
                "category",
                "processing-context");

            Assert.Equal(
                "component",
                component.Metadata["category"]);

            Assert.Equal(
                "processing-context",
                component.State.Context.Metadata["category"]);
        }

        [Fact]
        public void Base_object_shortcuts_resolve_to_component_owned_properties()
        {
            var component = new TestComponent("test");

            BaseObject baseObject = component;

            Assert.Same(component, baseObject.Component);
            Assert.Same(component.State, baseObject.State);
            Assert.Same(component.Signal, baseObject.Signal);
            Assert.Same(component.Metadata, baseObject.Metadata);
        }

        [Fact]
        public void Context_rejects_invalid_variable_keys()
        {
            var component = new TestComponent("test");

            Assert.Throws<ArgumentException>(
                () => component.State.Context.SetVariable(
                    " ",
                    new ConsumerThing("Invalid", 0)));
        }

        [Fact]
        public void State_rejects_unknown_instruction_types()
        {
            var component = new TestComponent("test");

            var processed = component.State.Process(
                new UnknownInstruction());

            Assert.False(processed);
            Assert.Empty(component.State.Context.Variables);
        }
    }

    // ========================================================
    // Consumer-owned runtime types
    //
    // WildsEngine.Core has no knowledge of these types.
    // They exist only inside the consuming test assembly.
    // ========================================================

    public sealed class ConsumerThing
    {
        public ConsumerThing(
            string name,
            int value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public int Value { get; }
    }

    public readonly struct ConsumerVector :
        IEquatable<ConsumerVector>
    {
        public ConsumerVector(
            float x,
            float y,
            float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public bool Equals(ConsumerVector other)
        {
            return X.Equals(other.X)
                && Y.Equals(other.Y)
                && Z.Equals(other.Z);
        }

        public override bool Equals(object? obj)
        {
            return obj is ConsumerVector other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = (hash * 31) + X.GetHashCode();
                hash = (hash * 31) + Y.GetHashCode();
                hash = (hash * 31) + Z.GetHashCode();

                return hash;
            }
        }
    }

    // ========================================================
    // Test instruction types
    // ========================================================

    public sealed class SetContextVariableInstruction :
        IStateInstruction
    {
        public SetContextVariableInstruction(
            string key,
            object? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public object? Value { get; }
    }

    public sealed class UnknownInstruction :
        IStateInstruction
    {
    }

    // ========================================================
    // Test component
    // ========================================================

    public sealed class TestComponent :
        BaseComponent
    {
        private readonly IState _state;
        private readonly ISignal _signal;

        public TestComponent(string layer)
            : base(layer)
        {
            var context = new TestContext(
                this,
                $"{layer}.context");

            _state = new TestState(
                this,
                context,
                $"{layer}.state");

            _signal = new TestSignal(
                this,
                $"{layer}.signal");
        }

        public override IState State => _state;

        public override ISignal Signal => _signal;
    }

    // ========================================================
    // Test context
    // ========================================================

    public sealed class TestContext :
        BaseObject,
        IContext
    {
        private readonly IComponent _component;

        private readonly Dictionary<string, object?> _variables =
            new Dictionary<string, object?>();

        private readonly Dictionary<string, object?>
            _contextMetadata =
                new Dictionary<string, object?>();

        public TestContext(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));
        }

        public override IComponent Component =>
            _component;

        public IReadOnlyDictionary<string, object?> Variables =>
            _variables;

        IReadOnlyDictionary<string, object?> IContext.Metadata =>
            _contextMetadata;

        public void SetVariable(
            string key,
            object? value)
        {
            ValidateKey(key);

            _variables[key] = value;
        }

        public void SetMetadata(
            string key,
            object? value)
        {
            ValidateKey(key);

            _contextMetadata[key] = value;
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Context keys cannot be empty.",
                    nameof(key));
        }
    }

    // ========================================================
    // Test state processor
    // ========================================================

    public sealed class TestState :
        BaseObject,
        IState
    {
        private readonly IComponent _component;

        public TestState(
            IComponent component,
            IContext context,
            string layer)
            : base(layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));

            Context = context
                ?? throw new ArgumentNullException(
                    nameof(context));
        }

        public override IComponent Component =>
            _component;

        public IContext Context { get; }

        public bool Process(
            IStateInstruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(
                    nameof(instruction));

            var setVariable =
                instruction as SetContextVariableInstruction;

            if (setVariable == null)
                return false;

            Context.SetVariable(
                setVariable.Key,
                setVariable.Value);

            return true;
        }
    }

    // ========================================================
    // Test signal
    //
    // ISignal intentionally has no additional contract yet.
    // ========================================================

    public sealed class TestSignal :
        BaseObject,
        ISignal
    {
        private readonly IComponent _component;

        public TestSignal(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));
        }

        public override IComponent Component =>
            _component;
    }
}

