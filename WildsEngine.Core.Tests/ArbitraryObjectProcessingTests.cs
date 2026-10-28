using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Diagnostics;
using WildsEngine.Core.Objects;
using WildsEngine.Core.Processing;
using Xunit;
using Xunit.Abstractions;

namespace WildsEngine.Core.Tests
{
    public sealed class ArbitraryObjectProcessingTests :
        IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Action<string>? _previousWriter;
        private readonly bool _previousEnabled;

        public ArbitraryObjectProcessingTests(
            ITestOutputHelper output)
        {
            _output = output
                ?? throw new ArgumentNullException(
                    nameof(output));

            _previousWriter = WildsLog.Writer;
            _previousEnabled = WildsLog.Enabled;

            WildsLog.Enabled = true;
            WildsLog.Writer = _output.WriteLine;
        }

        public void Dispose()
        {
            WildsLog.Writer = _previousWriter;
            WildsLog.Enabled = _previousEnabled;
        }

        [Fact]
        public void State_processes_external_reference_type_without_core_knowledge()
        {
            WildsLog.Info(
                "Starting external reference-type processing test.");

            var component = new TestComponent("test");

            var payload = new ConsumerThing(
                "External Consumer Object",
                42);

            WildsLog.Process(
                $"Created consumer-owned payload. " +
                $"Type={payload.GetType().FullName}, " +
                $"Name={payload.Name}, " +
                $"Value={payload.Value}.");

            var instruction = new SetContextVariableInstruction(
                "consumer",
                payload);

            WildsLog.Process(
                $"Processing instruction. " +
                $"Id={instruction.Id}, " +
                $"Intent={instruction.Intent}, " +
                $"Key={instruction.Key}.");

            var processed =
                component.State.Process(instruction);

            WildsLog.State(
                $"Instruction processed. " +
                $"Result={processed}, " +
                $"VariableExists=" +
                $"{component.State.Context.Variables.ContainsKey("consumer")}.");

            Assert.True(processed);
            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "consumer"));

            var stored =
                component.State.Context.Variables["consumer"];

            WildsLog.State(
                $"Retrieved stored value. " +
                $"Type={stored?.GetType().FullName ?? "<null>"}.");

            Assert.Same(payload, stored);

            var typed =
                Assert.IsType<ConsumerThing>(stored);

            Assert.Equal(
                "External Consumer Object",
                typed.Name);

            Assert.Equal(42, typed.Value);

            WildsLog.Info(
                "External reference-type processing test passed.");
        }

        [Fact]
        public void State_processes_external_value_type_without_core_knowledge()
        {
            WildsLog.Info(
                "Starting external value-type processing test.");

            var component = new TestComponent("test");

            var payload = new ConsumerVector(
                12.5f,
                -4.25f,
                81.75f);

            WildsLog.Process(
                $"Created consumer-owned value payload. " +
                $"Type={payload.GetType().FullName}, " +
                $"X={payload.X}, " +
                $"Y={payload.Y}, " +
                $"Z={payload.Z}.");

            var instruction = new SetContextVariableInstruction(
                "position",
                payload);

            WildsLog.Process(
                $"Processing instruction. " +
                $"Id={instruction.Id}, " +
                $"Intent={instruction.Intent}, " +
                $"Key={instruction.Key}.");

            var processed =
                component.State.Process(instruction);

            WildsLog.State(
                $"Instruction processed. " +
                $"Result={processed}, " +
                $"VariableExists=" +
                $"{component.State.Context.Variables.ContainsKey("position")}.");

            Assert.True(processed);
            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "position"));

            var stored =
                component.State.Context.Variables["position"];

            WildsLog.State(
                $"Retrieved stored value. " +
                $"Type={stored?.GetType().FullName ?? "<null>"}.");

            var typed =
                Assert.IsType<ConsumerVector>(stored);

            Assert.Equal(payload, typed);
            Assert.Equal(12.5f, typed.X);
            Assert.Equal(-4.25f, typed.Y);
            Assert.Equal(81.75f, typed.Z);

            WildsLog.Info(
                "External value-type processing test passed.");
        }

        [Fact]
        public void State_can_replace_an_existing_arbitrary_runtime_value()
        {
            WildsLog.Info(
                "Starting arbitrary runtime-value replacement test.");

            var component = new TestComponent("test");

            var original = new ConsumerThing(
                "Original",
                1);

            var replacement = new ConsumerThing(
                "Replacement",
                2);

            var originalInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    original);

            WildsLog.Process(
                $"Processing original value. " +
                $"InstructionId={originalInstruction.Id}, " +
                $"Name={original.Name}, " +
                $"Value={original.Value}.");

            Assert.True(
                component.State.Process(
                    originalInstruction));

            var replacementInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    replacement);

            WildsLog.Process(
                $"Processing replacement value. " +
                $"InstructionId={replacementInstruction.Id}, " +
                $"Name={replacement.Name}, " +
                $"Value={replacement.Value}.");

            Assert.True(
                component.State.Process(
                    replacementInstruction));

            var stored =
                component.State.Context.Variables["subject"];

            WildsLog.State(
                $"Replacement completed. " +
                $"StoredType=" +
                $"{stored?.GetType().FullName ?? "<null>"}.");

            Assert.Same(replacement, stored);
            Assert.NotSame(original, stored);

            var typed =
                Assert.IsType<ConsumerThing>(stored);

            Assert.Equal("Replacement", typed.Name);
            Assert.Equal(2, typed.Value);

            WildsLog.Info(
                "Arbitrary runtime-value replacement test passed.");
        }

        [Fact]
        public void State_processing_can_transition_existing_value_to_null()
        {
            WildsLog.Info(
                "Starting value-to-null transition test.");

            var component = new TestComponent("test");

            var original = new ConsumerThing(
                "Original",
                1);

            var originalInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    original);

            WildsLog.Process(
                $"Setting initial non-null value. " +
                $"InstructionId={originalInstruction.Id}.");

            Assert.True(
                component.State.Process(
                    originalInstruction));

            Assert.Same(
                original,
                component.State.Context.Variables["subject"]);

            var nullInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    null);

            WildsLog.Process(
                $"Transitioning existing value to null. " +
                $"InstructionId={nullInstruction.Id}.");

            Assert.True(
                component.State.Process(
                    nullInstruction));

            var keyExists =
                component.State.Context.Variables.ContainsKey(
                    "subject");

            var stored =
                component.State.Context.Variables["subject"];

            WildsLog.State(
                $"Value-to-null transition completed. " +
                $"KeyExists={keyExists}, " +
                $"StoredIsNull={stored == null}.");

            Assert.True(keyExists);
            Assert.Null(stored);

            WildsLog.Info(
                "Value-to-null transition test passed.");
        }

        [Fact]
        public void State_processing_can_transition_null_to_consumer_object()
        {
            WildsLog.Info(
                "Starting null-to-consumer-object transition test.");

            var component = new TestComponent("test");

            var replacement = new ConsumerThing(
                "Replacement",
                2);

            var nullInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    null);

            WildsLog.Process(
                $"Creating present-null variable. " +
                $"InstructionId={nullInstruction.Id}.");

            Assert.True(
                component.State.Process(
                    nullInstruction));

            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "subject"));

            Assert.Null(
                component.State.Context.Variables["subject"]);

            var replacementInstruction =
                new SetContextVariableInstruction(
                    "subject",
                    replacement);

            WildsLog.Process(
                $"Transitioning null value to consumer object. " +
                $"InstructionId={replacementInstruction.Id}, " +
                $"Name={replacement.Name}, " +
                $"Value={replacement.Value}.");

            Assert.True(
                component.State.Process(
                    replacementInstruction));

            var stored =
                component.State.Context.Variables["subject"];

            WildsLog.State(
                $"Null-to-object transition completed. " +
                $"StoredType=" +
                $"{stored?.GetType().FullName ?? "<null>"}.");

            Assert.Same(replacement, stored);

            var typed =
                Assert.IsType<ConsumerThing>(stored);

            Assert.Equal("Replacement", typed.Name);
            Assert.Equal(2, typed.Value);

            WildsLog.Info(
                "Null-to-consumer-object transition test passed.");
        }

        [Fact]
        public void Context_metadata_remains_separate_from_component_metadata()
        {
            WildsLog.Info(
                "Starting metadata ownership-separation test.");

            var component = new TestComponent("test");

            component.Metadata["category"] =
                "component";

            WildsLog.State(
                "Set component-owned metadata: " +
                "category=component.");

            component.State.Context.SetMetadata(
                "category",
                "processing-context");

            WildsLog.State(
                "Set context-owned metadata: " +
                "category=processing-context.");

            var componentCategory =
                component.Metadata["category"];

            var contextCategory =
                component.State.Context.Metadata["category"];

            WildsLog.State(
                $"Resolved metadata values. " +
                $"Component={componentCategory}, " +
                $"Context={contextCategory}.");

            Assert.Equal(
                "component",
                componentCategory);

            Assert.Equal(
                "processing-context",
                contextCategory);

            WildsLog.Info(
                "Metadata ownership-separation test passed.");
        }

        [Fact]
        public void Base_object_shortcuts_resolve_to_component_owned_properties()
        {
            WildsLog.Info(
                "Starting BaseObject shortcut-resolution test.");

            var component = new TestComponent("test");

            BaseObject baseObject = component;

            WildsLog.State(
                $"Resolving shortcuts through component. " +
                $"ComponentId={component.Id}, " +
                $"ComponentSlug={component.Slug}.");

            Assert.Same(
                component,
                baseObject.Component);

            Assert.Same(
                component.State,
                baseObject.State);

            Assert.Same(
                component.Signal,
                baseObject.Signal);

            Assert.Same(
                component.Metadata,
                baseObject.Metadata);

            WildsLog.Info(
                "BaseObject shortcut-resolution test passed.");
        }

        [Fact]
        public void Context_rejects_invalid_variable_keys()
        {
            WildsLog.Info(
                "Starting invalid context-key rejection test.");

            var component = new TestComponent("test");

            WildsLog.Process(
                "Attempting to set a context variable " +
                "using a whitespace-only key.");

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        component.State.Context.SetVariable(
                            " ",
                            new ConsumerThing(
                                "Invalid",
                                0)));

            WildsLog.Warning(
                $"Invalid key was rejected. " +
                $"ExceptionType={exception.GetType().FullName}, " +
                $"Parameter={exception.ParamName}.");

            Assert.Empty(
                component.State.Context.Variables);

            WildsLog.Info(
                "Invalid context-key rejection test passed.");
        }

        [Fact]
        public void State_rejects_unknown_instruction_types()
        {
            WildsLog.Info(
                "Starting unknown-instruction rejection test.");

            var component = new TestComponent("test");

            IStateInstruction instruction =
                new UnknownInstructionBase();

            WildsLog.Process(
                $"Processing unknown instruction. " +
                $"Type={instruction.GetType().FullName}, " +
                $"Id={instruction.Id}, " +
                $"Intent={instruction.Intent}.");

            var processed =
                component.State.Process(instruction);

            WildsLog.Warning(
                $"Unknown instruction processing completed. " +
                $"Accepted={processed}.");

            Assert.False(processed);
            Assert.Empty(
                component.State.Context.Variables);

            WildsLog.Info(
                "Unknown-instruction rejection test passed.");
        }

        [Fact]
        public void State_processes_external_instruction_and_payload_without_core_knowledge()
        {
            WildsLog.Info(
                "Starting external instruction and payload test.");

            var component = new TestComponent("test");

            var payload = new ConsumerVector(
                7.5f,
                -2.25f,
                19.0f);

            WildsLog.Process(
                $"Created external payload. " +
                $"Type={payload.GetType().FullName}, " +
                $"X={payload.X}, " +
                $"Y={payload.Y}, " +
                $"Z={payload.Z}.");

            IStateInstruction instruction =
                new ConsumerSetVariableInstructionBase(
                    "velocity",
                    payload);

            WildsLog.Process(
                $"Created external instruction. " +
                $"Type={instruction.GetType().FullName}, " +
                $"Id={instruction.Id}, " +
                $"Intent={instruction.Intent}, " +
                $"PayloadType=" +
                $"{instruction.Payload?.GetType().FullName ?? "<null>"}.");

            var processed =
                component.State.Process(instruction);

            WildsLog.State(
                $"External instruction processed. " +
                $"Result={processed}, " +
                $"VariableExists=" +
                $"{component.State.Context.Variables.ContainsKey("velocity")}.");

            Assert.True(processed);

            Assert.True(
                component.State.Context.Variables.ContainsKey(
                    "velocity"));

            var stored =
                component.State.Context.Variables["velocity"];

            WildsLog.State(
                $"Retrieved external payload from context. " +
                $"StoredType=" +
                $"{stored?.GetType().FullName ?? "<null>"}.");

            var typed =
                Assert.IsType<ConsumerVector>(stored);

            Assert.Equal(payload, typed);
            Assert.Equal(7.5f, typed.X);
            Assert.Equal(-2.25f, typed.Y);
            Assert.Equal(19.0f, typed.Z);

            Assert.NotEqual(
                Guid.Empty,
                instruction.Id);

            Assert.Equal(
                "consumer.context.variable.set",
                instruction.Intent);

            Assert.Equal(
                payload,
                instruction.Payload);

            WildsLog.Info(
                "External instruction and payload test passed.");
        }
    }
}