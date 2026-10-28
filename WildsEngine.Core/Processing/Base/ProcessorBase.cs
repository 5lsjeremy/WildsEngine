using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    /// <summary>
    /// Base abstraction for Wilds processing infrastructure.
    /// Provides the common object surface used by task and
    /// instruction processors.
    /// </summary>
    public abstract class ProcessorBase :
        BaseObject,
        IResettablePoolObject
    {
        protected ProcessorBase(string layer)
            : base(layer)
        {
        }

        public abstract override IComponent Component { get; }

        public bool TryReset()
        {
            return OnReset();
        }

        /// <summary>
        /// Clears transient processing state before this processor
        /// is retained by its pool.
        /// </summary>
        protected virtual bool OnReset()
        {
            return true;
        }
    }
}