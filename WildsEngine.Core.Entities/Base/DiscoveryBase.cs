using System;
using WildsEngine.Core.Entities.Base;

public abstract class DiscoveryBase :
    IdentifierBase,
    IDiscovery
{
    protected DiscoveryBase(
        string slug,
        IFact fact,
        bool isAvailable = false)
        : base(
            slug)
    {
        Fact =
            fact ??
            throw new ArgumentNullException(
                nameof(fact));

        IsAvailable =
            isAvailable;
    }

    protected DiscoveryBase(
        Guid id,
        string slug,
        IFact fact,
        bool isAvailable = false)
        : base(
            id,
            slug)
    {
        Fact =
            fact ??
            throw new ArgumentNullException(
                nameof(fact));

        IsAvailable =
            isAvailable;
    }

    public IFact Fact { get; }

    public bool IsAvailable { get; private set; }

    protected void MakeAvailable()
    {
        IsAvailable =
            true;
    }

    protected void MakeUnavailable()
    {
        IsAvailable =
            false;
    }
}