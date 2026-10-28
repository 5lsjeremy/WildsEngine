using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Processing.Registry;
using WildsEngine.Core.Tests;

namespace WildsEngine.Tests.Registry
{
    /// <summary>
    /// Shared component and registry composition used by
    /// registry certification tests.
    /// </summary>
    public sealed class RegistryTestFixture
    {
        public RegistryTestFixture(
            string layer = "certification.registry")
        {
            Component =
                new TestComponent(
                    $"{layer}.component");
        }

        public IComponent Component { get; }

        public DefaultTasksRegistry CreateTaskRegistry(
            string layer)
        {
            return new DefaultTasksRegistry(
                Component,
                layer);
        }

        public TestInstructionRegistry CreateInstructionRegistry(
            string layer)
        {
            return new TestInstructionRegistry(
                Component,
                layer);
        }

        public TestTask CreateTask(
            string layer)
        {
            return new TestTask(
                Component,
                layer);
        }

        public sealed class TestInstructionRegistry :
            RegistryBase<IStateInstruction>
        {
            public TestInstructionRegistry(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer,
                    instruction => instruction.Id)
            {
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnShutdown()
            {
                Clear();
            }
        }

        public sealed class TestTask :
            TaskBase
        {
            public TestTask(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer)
            {
            }
        }
    }
}