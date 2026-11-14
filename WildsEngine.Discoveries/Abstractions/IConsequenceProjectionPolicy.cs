using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Consumer-supplied policy that projects possible
    /// Consequences from scoped Truths.
    /// </summary>
    public interface IConsequenceProjectionPolicy
    {
        IReadOnlyList<IConsequence> Project(
            IReadOnlyList<ITruth> truths);
    }
}
