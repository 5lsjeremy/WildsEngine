namespace WildsEngine.Core.Abstractions
{
    public interface IRegistry :
        IBaseObject
    {
        bool IsInitialized { get; }

        bool Initialize();

        bool Shutdown();
    }
}