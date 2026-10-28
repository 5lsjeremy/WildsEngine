namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Provides bounded leasing of task processors.
    /// </summary>
    public interface ITaskProcessorPool :
        IObjectPool<ITaskProcessor>
    {
    }
}