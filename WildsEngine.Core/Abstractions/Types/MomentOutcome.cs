namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Describes how a Pulse resolved into a Moment.
    /// </summary>
    public enum MomentOutcome
    {
        Completed = 0,
        Failed,
        Cancelled
    }
}