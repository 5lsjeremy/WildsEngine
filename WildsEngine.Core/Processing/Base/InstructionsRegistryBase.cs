using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public abstract class InstructionsRegistryBase :
        RegistryBase,
        Abstractions.IInstructionsRegistry
    {
        protected InstructionsRegistryBase(string layer)
            : base(layer)
        {
        }
    }
}