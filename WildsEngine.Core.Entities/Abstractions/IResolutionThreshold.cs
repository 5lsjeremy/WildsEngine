using System.Collections.Generic;

namespace WildsEngine.Core.Entities.Abstractions
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
