using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Surface
{
    internal sealed class DiscoveryGeometryRegistrationCatalog
    {
        private readonly ConcurrentDictionary<Guid, IDiscoveryGeometryRegistration> _byId = new ConcurrentDictionary<Guid, IDiscoveryGeometryRegistration>();
        private readonly ConcurrentDictionary<Guid, IDiscoveryGeometryRegistration> _byProfile = new ConcurrentDictionary<Guid, IDiscoveryGeometryRegistration>();
        public void Register(IDiscoveryGeometryRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (!_byProfile.TryAdd(registration.ProfileId, registration)) throw new InvalidOperationException("This geometry profile is already registered.");
            if (!_byId.TryAdd(registration.Id, registration))
            {
                _byProfile.TryRemove(registration.ProfileId, out _);
                throw new InvalidOperationException("A geometry registration with this ID already exists.");
            }
        }
        public bool TryGet(Guid id, out IDiscoveryGeometryRegistration? registration) => _byId.TryGetValue(id, out registration);
        public IReadOnlyList<IDiscoveryGeometryRegistration> All
        {
            get
            {
                var result = new List<IDiscoveryGeometryRegistration>(_byId.Values);
                result.Sort((left, right) => left.RegisteredAt.CompareTo(right.RegisteredAt));
                return result.AsReadOnly();
            }
        }
    }
}
