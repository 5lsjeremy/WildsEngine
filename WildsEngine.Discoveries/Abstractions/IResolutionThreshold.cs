using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    public interface IResolutionThreshold<out TContributor>
    {
        IReadOnlyList<TContributor> Contributors { get; }
        int RequiredCount { get; }
        int CurrentCount { get; }
        double Percentage { get; }
        bool IsResolved { get; }
    }
}
