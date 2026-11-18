using System;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IWildsDiscoveriesRegistration : IIdentifier
    {
        Guid ProfileId { get; }
        string ProfileSlug { get; }
        string ProfileVersion { get; }
        string Scope { get; }
        DateTimeOffset RegisteredAt { get; }
    }
}
