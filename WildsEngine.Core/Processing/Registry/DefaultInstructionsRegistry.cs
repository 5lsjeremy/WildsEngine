using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Registry
{
    public sealed class DefaultInstructionsRegistry :
        InstructionsRegistryBase
    {
        public DefaultInstructionsRegistry(
            IComponent component,
            string layer)
            : base(
                component,
                layer)
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
}