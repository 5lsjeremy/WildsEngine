namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Describes the lifecycle of one internal piece of work
    /// required to resolve a pulse.
    /// </summary>
    public enum WorkUnitStatus
    {
        Created = 0,
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
