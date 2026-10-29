namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Describes the lifecycle of one complete request
    /// submitted to Wilds.
    /// </summary>
    public enum PulseStatus
    {
        Created = 0,
        Decomposing,
        Pending,
        Ready,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
