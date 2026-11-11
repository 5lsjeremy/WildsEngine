namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Optional Wilds-compatible projection used only when Evidence must
    /// independently participate in Wilds processing.
    /// </summary>
    public interface IWildsEvidence :
        IWildsProjection<IDiscoveryEvidence>
    {
        IDiscoveryEvidence Evidence { get; }
    }
}
