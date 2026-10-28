using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public abstract class InstructionsRegistryBase :
        RegistryBase<IStateInstruction>,
        IInstructionsRegistry
    {
        protected InstructionsRegistryBase(
            IComponent component,
            string layer)
            : base(
                component,
                layer,
                instruction => instruction.Id)
        {
        }
    }
}