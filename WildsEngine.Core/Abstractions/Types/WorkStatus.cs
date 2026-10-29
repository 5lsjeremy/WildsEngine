namespace WildsEngine.Core.Abstractions
{
    public enum WorkStatus
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