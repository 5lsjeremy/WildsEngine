using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    /// <summary>
    /// Base implementation for task registries.
    /// </summary>
    public abstract class TaskRegistryBase :
        RegistryBase<ITask>,
        ITaskRegistry
    {
        protected TaskRegistryBase(
            IComponent component,
            string layer)
            : base(
                component,
                layer,
                task => task.Id)
        {
        }
    }
}