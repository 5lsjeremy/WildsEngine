namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceResult<out TResult>
        where TResult : class
    {
        System.Guid ExecutionId { get; }
        TResult Value { get; }
    }
}
