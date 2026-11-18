using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Surface;
using WildsEngine.Discoveries.Narratives;

namespace WildsEngine.Discoveries.Surface
{
    internal sealed class DiscoveryNarrativeSurface : IDiscoveryNarrativeSurface
    {
        private readonly IDiscoveryGeometrySurface _geometry;
        private readonly IDiscoveryRecognizedGeometryPopulationFactory _populations;
        private readonly IWildsServiceClock _clock;
        private readonly ConcurrentDictionary<Guid, IDiscoveryNarrativeProfile> _profiles = new ConcurrentDictionary<Guid, IDiscoveryNarrativeProfile>();
        private readonly ConcurrentDictionary<Guid, IDiscoveryNarrativeRegistration> _registrations = new ConcurrentDictionary<Guid, IDiscoveryNarrativeRegistration>();

        public DiscoveryNarrativeSurface(
            IDiscoveryGeometrySurface geometry,
            IDiscoveryRecognizedGeometryPopulationFactory populations,
            IWildsServiceClock clock)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _populations = populations ?? throw new ArgumentNullException(nameof(populations));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IDiscoveryRecognizedGeometryPopulation CreatePopulation(
            Guid geometryRegistrationId,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> recognizedGeometry)
        {
            if (!_geometry.TryGetRegistration(geometryRegistrationId, out var registration) || registration == null)
                throw new KeyNotFoundException("No geometry registration exists for ID: " + geometryRegistrationId);
            return _populations.Create(registration, recognizedGeometry);
        }

        public IDiscoveryNarrativeRegistration Register(IDiscoveryNarrativeProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var id = Guid.NewGuid();
            var registration = new DiscoveryNarrativeRegistration(id, profile, _clock.GetUtcNow());
            if (!_profiles.TryAdd(profile.Id, profile))
                throw new InvalidOperationException("The narrative profile is already registered.");
            if (!_registrations.TryAdd(registration.Id, registration))
            {
                _profiles.TryRemove(profile.Id, out _);
                throw new InvalidOperationException("The narrative registration could not be installed.");
            }
            return registration;
        }

        public IDiscoveryNarrativeCompositionResult Compose(
            IDiscoveryRecognizedGeometryPopulation population,
            Guid narrativeRegistrationId)
        {
            if (population == null) throw new ArgumentNullException(nameof(population));
            if (!_registrations.TryGetValue(narrativeRegistrationId, out var registration))
                throw new KeyNotFoundException("No narrative registration exists for ID: " + narrativeRegistrationId);
            if (!_profiles.TryGetValue(registration.ProfileId, out var profile))
                throw new KeyNotFoundException("No narrative profile exists for registered profile ID: " + registration.ProfileId);
            if (!string.Equals(population.Scope, registration.Scope, StringComparison.Ordinal))
                throw new InvalidOperationException("Population and narrative registration scopes must match.");

            var groups = new List<List<IDiscoveryClusterRecognitionResult>>();
            var decisions = new List<IDiscoveryNarrativeCompositionDecision>();
            foreach (var candidate in population.Members)
            {
                var placed = false;
                for (var groupIndex = 0; groupIndex < groups.Count && !placed; groupIndex++)
                {
                    if (CanJoinGroup(candidate, groups[groupIndex], profile, decisions))
                    {
                        groups[groupIndex].Add(candidate);
                        placed = true;
                    }
                }
                if (!placed) groups.Add(new List<IDiscoveryClusterRecognitionResult> { candidate });
            }

            var output = new List<IDiscoveryNarrativeGroup>(groups.Count);
            foreach (var group in groups) output.Add(new DiscoveryNarrativeGroup(group));
            return new DiscoveryNarrativeCompositionResult(
                Guid.NewGuid(), population, registration, output, decisions, _clock.GetUtcNow());
        }

        private static bool CanJoinGroup(
            IDiscoveryClusterRecognitionResult candidate,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> group,
            IDiscoveryNarrativeProfile profile,
            List<IDiscoveryNarrativeCompositionDecision> decisions)
        {
            var accepted = true;
            for (var index = 0; index < group.Count; index++)
            {
                var member = group[index];
                var measurement = profile.Metric.Measure(member, candidate);
                if (double.IsNaN(measurement) || double.IsInfinity(measurement))
                    throw new InvalidOperationException("The narrative metric returned a non-finite measurement.");
                var relationship = FindRelationship(
                    profile, member.Mapping.SemanticKey, candidate.Mapping.SemanticKey);
                var canCompose = profile.CompositionPolicy.CanCompose(
                    member, candidate, measurement, relationship);
                decisions.Add(new DiscoveryNarrativeCompositionDecision(
                    member, candidate, measurement, relationship, canCompose));
                if (!canCompose) accepted = false;
            }
            return accepted;
        }

        private static IDiscoveryNarrativeRelationshipDefinition? FindRelationship(
            IDiscoveryNarrativeProfile profile,
            string leftSemanticKey,
            string rightSemanticKey)
        {
            IDiscoveryNarrativeRelationshipDefinition? match = null;
            foreach (var relationship in profile.Relationships)
            {
                var matches =
                    (string.Equals(relationship.LeftSemanticKey, leftSemanticKey, StringComparison.Ordinal) &&
                     string.Equals(relationship.RightSemanticKey, rightSemanticKey, StringComparison.Ordinal)) ||
                    (string.Equals(relationship.LeftSemanticKey, rightSemanticKey, StringComparison.Ordinal) &&
                     string.Equals(relationship.RightSemanticKey, leftSemanticKey, StringComparison.Ordinal));
                if (!matches) continue;
                if (match != null)
                    throw new InvalidOperationException("The narrative profile contains multiple relationships for the same semantic pair.");
                match = relationship;
            }
            return match;
        }
    }
}
