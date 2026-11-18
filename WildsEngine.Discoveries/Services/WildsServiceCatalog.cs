using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceCatalog : IWildsServiceCatalog
    {
        private readonly ConcurrentDictionary<Guid, IIdentifier> _byId = new ConcurrentDictionary<Guid, IIdentifier>();
        private readonly ConcurrentDictionary<string, IIdentifier> _bySlug =
            new ConcurrentDictionary<string, IIdentifier>(StringComparer.Ordinal);

        public IReadOnlyList<IIdentifier> Services => new List<IIdentifier>(_byId.Values).AsReadOnly();

        public void Register(IIdentifier service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (!_byId.TryAdd(service.Id, service)) throw new InvalidOperationException("A service with this ID is already registered.");
            if (!_bySlug.TryAdd(service.Slug, service))
            {
                _byId.TryRemove(service.Id, out _);
                throw new InvalidOperationException("A service with this slug is already registered.");
            }
        }

        public bool TryGet(Guid serviceId, out IIdentifier? service) => _byId.TryGetValue(serviceId, out service);
        public bool TryGet(string slug, out IIdentifier? service)
        {
            if (string.IsNullOrWhiteSpace(slug)) { service = null; return false; }
            return _bySlug.TryGetValue(slug, out service);
        }
    }
}
