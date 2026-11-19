using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Truths;

namespace WildsEngine.Discoveries.Truths
{
    internal sealed class DiscoveryTruthSurface :
        IDiscoveryTruthSurface
    {
        private readonly IWildsServiceClock _clock;

        private readonly ConcurrentDictionary<
            Guid,
            IDiscoveryTruthProfile> _profiles =
                new ConcurrentDictionary<
                    Guid,
                    IDiscoveryTruthProfile>();

        private readonly ConcurrentDictionary<
            Guid,
            IDiscoveryTruthRegistration> _registrations =
                new ConcurrentDictionary<
                    Guid,
                    IDiscoveryTruthRegistration>();

        public DiscoveryTruthSurface(
            IWildsServiceClock clock)
        {
            _clock =
                clock ??
                throw new ArgumentNullException(
                    nameof(clock));
        }

        public IDiscoveryTruthRegistration Register(
            IDiscoveryTruthProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(
                    nameof(profile));
            }

            if (!(profile.CongealmentPolicy
                is IIdentifiedTruthCongealmentPolicy identifiedPolicy))
            {
                throw new ArgumentException(
                    "The Truth congealment policy must expose stable identity.",
                    nameof(profile));
            }

            if (identifiedPolicy.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "The Truth congealment policy requires a non-empty identity.",
                    nameof(profile));
            }

            var registration =
                new DiscoveryTruthRegistration(
                    Guid.NewGuid(),
                    profile,
                    identifiedPolicy.Id,
                    _clock.GetUtcNow());

            if (!_profiles.TryAdd(
                    profile.Id,
                    profile))
            {
                throw new InvalidOperationException(
                    "This Truth profile is already registered.");
            }

            if (!_registrations.TryAdd(
                    registration.Id,
                    registration))
            {
                _profiles.TryRemove(
                    profile.Id,
                    out _);

                throw new InvalidOperationException(
                    "The Truth registration could not be installed.");
            }

            return registration;
        }

        public IDiscoveryTruthCongealmentResult Congeal(
            IDiscoveryCompetingNarrativesResult narratives,
            Guid registrationId,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters)
        {
            if (narratives == null)
            {
                throw new ArgumentNullException(
                    nameof(narratives));
            }

            if (series == null)
            {
                throw new ArgumentNullException(
                    nameof(series));
            }

            if (clusters == null)
            {
                throw new ArgumentNullException(
                    nameof(clusters));
            }

            if (!_registrations.TryGetValue(
                    registrationId,
                    out var registration))
            {
                throw new KeyNotFoundException(
                    "No Truth registration exists for ID: " +
                    registrationId);
            }

            if (!_profiles.TryGetValue(
                    registration.ProfileId,
                    out var profile))
            {
                throw new KeyNotFoundException(
                    "No Truth profile exists for registered profile ID: " +
                    registration.ProfileId);
            }

            ValidateScope(
                narratives,
                series,
                registration);

            ValidateClusters(
                clusters);

            var canCongeal =
                profile.CongealmentPolicy.CanCongeal(
                    clusters,
                    series);

            var truths =
                new List<ITruth>();

            if (canCongeal)
            {
                var truth =
                    profile.CongealmentPolicy.Congeal(
                        clusters,
                        series);

                if (truth == null)
                {
                    throw new InvalidOperationException(
                        "The Truth congealment policy approved congealment " +
                        "but returned null.");
                }

                ValidateCongealedTruth(
                    truth,
                    series,
                    clusters);

                truths.Add(
                    truth);
            }

            return new DiscoveryTruthCongealmentResult(
                Guid.NewGuid(),
                narratives,
                registration,
                series,
                clusters,
                truths,
                canCongeal,
                _clock.GetUtcNow());
        }

        private static void ValidateScope(
            IDiscoveryCompetingNarrativesResult narratives,
            IDiscoveryVectorSeries series,
            IDiscoveryTruthRegistration registration)
        {
            var narrativeScope =
                narratives
                    .Composition
                    .Population
                    .Scope;

            if (!string.Equals(
                    narrativeScope,
                    registration.Scope,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Narrative and Truth registration scopes must match.");
            }

            if (!string.Equals(
                    series.Scope,
                    registration.Scope,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Vector-series and Truth registration scopes must match.");
            }
        }

        private static void ValidateClusters(
            IReadOnlyList<IDiscoveryCluster> clusters)
        {
            for (var clusterIndex = 0;
                 clusterIndex < clusters.Count;
                 clusterIndex++)
            {
                var cluster =
                    clusters[clusterIndex];

                if (cluster == null)
                {
                    throw new ArgumentException(
                        "Truth clusters cannot contain null.",
                        nameof(clusters));
                }

                if (cluster.Members == null ||
                    cluster.Members.Count == 0)
                {
                    throw new ArgumentException(
                        "Every Truth cluster requires at least one member.",
                        nameof(clusters));
                }
            }
        }

        private static void ValidateCongealedTruth(
            ITruth truth,
            IDiscoveryVectorSeries authoritativeSeries,
            IReadOnlyList<IDiscoveryCluster> authoritativeClusters)
        {
            if (!ReferenceEquals(
                    truth.Series,
                    authoritativeSeries))
            {
                throw new InvalidOperationException(
                    "The congealed Truth must retain the exact " +
                    "authoritative vector series.");
            }

            if (truth.Clusters == null)
            {
                throw new InvalidOperationException(
                    "The congealed Truth returned a null cluster collection.");
            }

            for (var truthClusterIndex = 0;
                 truthClusterIndex < truth.Clusters.Count;
                 truthClusterIndex++)
            {
                var truthCluster =
                    truth.Clusters[truthClusterIndex];

                if (truthCluster == null)
                {
                    throw new InvalidOperationException(
                        "The congealed Truth contains a null cluster.");
                }

                if (!ContainsExactCluster(
                        authoritativeClusters,
                        truthCluster))
                {
                    throw new InvalidOperationException(
                        "The congealed Truth references a cluster " +
                        "outside the exact congealment input.");
                }
            }
        }

        private static bool ContainsExactCluster(
            IReadOnlyList<IDiscoveryCluster> clusters,
            IDiscoveryCluster candidate)
        {
            for (var index = 0;
                 index < clusters.Count;
                 index++)
            {
                if (ReferenceEquals(
                        clusters[index],
                        candidate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}