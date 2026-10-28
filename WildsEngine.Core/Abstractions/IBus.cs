namespace WildsEngine.Core.Abstractions
{
    public interface IBus :
        IBaseObject
    {
        bool Dispatch(ISignal signal);
    }
}