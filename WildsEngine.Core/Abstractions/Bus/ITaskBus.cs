namespace WildsEngine.Core.Abstractions
{
    public interface ITaskBus :
        IBus
    {
        bool Dispatch(ITask task);
    }
}