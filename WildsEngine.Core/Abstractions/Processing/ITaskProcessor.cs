namespace WildsEngine.Core.Abstractions
{
    public interface ITaskProcessor
    {
        ITaskRegistry Registry { get; }
    }

    public interface ITaskRegistry
    {
    }
}