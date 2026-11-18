using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceCatalog
    {
        IReadOnlyList<IIdentifier> Services { get; }
        void Register(IIdentifier service);
        bool TryGet(Guid serviceId, out IIdentifier? service);
        bool TryGet(string slug, out IIdentifier? service);
    }
}
