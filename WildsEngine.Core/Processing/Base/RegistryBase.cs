using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class RegistryBase :
        BaseObject,
        IRegistry
    {
        protected RegistryBase(string layer)
            : base(layer)
        {
        }

        public bool IsInitialized { get; private set; }

        public bool Initialize()
        {
            if (IsInitialized)
            {
                return false;
            }

            OnInitialize();

            IsInitialized = true;

            return true;
        }

        public bool Shutdown()
        {
            if (!IsInitialized)
            {
                return false;
            }

            OnShutdown();

            IsInitialized = false;

            return true;
        }

        protected abstract void OnInitialize();

        protected abstract void OnShutdown();
    }
}