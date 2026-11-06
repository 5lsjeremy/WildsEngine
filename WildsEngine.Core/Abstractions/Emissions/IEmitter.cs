using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents an identifiable Wilds object capable of
    /// producing scalar emissions from consumer-owned data.
    ///
    /// As an IBaseObject, an emitter participates in Wilds
    /// topology through ordinary Neighbor relationships.
    /// </summary>
    public interface IEmitter :
        IBaseObject
    {
        IEmissionDataSource DataSource { get; }

        bool TryEmit(
            string propertyName,
            out double value);

        bool TryEmitGroup(
            string groupName,
            out IReadOnlyDictionary<string, double> values);
    }
}