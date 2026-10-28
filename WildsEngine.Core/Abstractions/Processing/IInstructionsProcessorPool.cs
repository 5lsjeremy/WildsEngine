namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Provides bounded leasing of instruction processors.
    /// </summary>
    public interface IInstructionsProcessorPool :
        IObjectPool<IInstructionsProcessor>
    {
    }
}