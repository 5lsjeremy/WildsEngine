namespace WildsEngine.Discoveries.Abstractions.Services
{
    public enum WildsServiceExecutionOutcome
    {
        Requested = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Cancelled = 4
    }
}
