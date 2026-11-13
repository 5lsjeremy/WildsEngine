using WildsEngine.Core.Abstractions;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Projects an authoritative consumer-facing domain object into Wilds as an
    /// ordinary processable object. Projection adds Wilds identity and processing
    /// participation; it does not replace or reinterpret the source.
    /// </summary>
    public interface IWildsProjection<out TSource> :
        IBaseObject
        where TSource : class
    {
        TSource Source { get; }
    }
}
