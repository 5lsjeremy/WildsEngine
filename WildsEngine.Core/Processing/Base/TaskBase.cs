using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class TaskBase :
        BaseObject,
        ITask
    {
        protected TaskBase(string layer)
            : base(layer)
        {
        }
    }
}