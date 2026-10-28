using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public abstract class TaskRegistryBase :
        RegistryBase,
        ITaskRegistry
    {
        protected TaskRegistryBase(string layer)
            : base(layer)
        {
        }
    }
}