using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Truths;

namespace WildsEngine.Discoveries.Consequences
{
    internal sealed class DiscoveryConsequenceSurface :
        IDiscoveryConsequenceSurface
    {
        private readonly IWildsServiceClock _clock;

        private readonly ConcurrentDictionary<
            Guid,
            IDiscoveryConsequenceProfile> _profiles =
                new ConcurrentDictionary<
                    Guid,
                    IDiscoveryConsequenceProfile>();

        private readonly ConcurrentDictionary<
            Guid,
            IDiscoveryConsequenceRegistration> _registrations =
                new ConcurrentDictionary<
                    Guid,
                    IDiscoveryConsequenceRegistration>();

        public DiscoveryConsequenceSurface(
            IWildsServiceClock clock)
        {
            _clock =
                clock ??
                throw new ArgumentNullException(
                    nameof(clock));
        }

        public IDiscoveryConsequenceRegistration Register(
            IDiscoveryConsequenceProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(
                    nameof(profile));
            }

            if (!(profile.ProjectionPolicy
                is IIdentifiedConsequenceProjectionPolicy identifiedPolicy))
            {
                throw new ArgumentException(
                    "The Consequence projection policy must expose stable identity.",
                    nameof(profile));
            }

            if (identifiedPolicy.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "The Consequence projection policy requires a non-empty identity.",
                    nameof(profile));
            }

            var registration =
                new DiscoveryConsequenceRegistration(
                    Guid.NewGuid(),
                    profile,
                    identifiedPolicy.Id,
                    _clock.GetUtcNow());

            if (!_profiles.TryAdd(
                    profile.Id,
                    profile))
            {
                throw new InvalidOperationException(
                    "This Consequence profile is already registered.");
            }

            if (!_registrations.TryAdd(
                    registration.Id,
                    registration))
            {
                _profiles.TryRemove(
                    profile.Id,
                    out _);

                throw new InvalidOperationException(
                    "The Consequence registration could not be installed.");
            }

            return registration;
        }

        public IDiscoveryConsequenceProjectionResult Project(
            IDiscoveryTruthCongealmentResult truthResult,
            Guid registrationId)
        {
            if (truthResult == null)
            {
                throw new ArgumentNullException(
                    nameof(truthResult));
            }

            if (!_registrations.TryGetValue(
                    registrationId,
                    out var registration))
            {
                throw new KeyNotFoundException(
                    "No Consequence registration exists for ID: " +
                    registrationId);
            }

            if (!_profiles.TryGetValue(
                    registration.ProfileId,
                    out var profile))
            {
                throw new KeyNotFoundException(
                    "No Consequence profile exists for registered profile ID: " +
                    registration.ProfileId);
            }

            if (!string.Equals(
                    truthResult.Registration.Scope,
                    registration.Scope,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Truth and Consequence registration scopes must match.");
            }

            var projected =
                profile.ProjectionPolicy.Project(
                    truthResult.Truths);

            if (projected == null)
            {
                throw new InvalidOperationException(
                    "The Consequence projection policy returned null.");
            }

            ValidateProjectedConsequences(
                truthResult.Truths,
                projected);

            return new DiscoveryConsequenceProjectionResult(
                Guid.NewGuid(),
                truthResult,
                registration,
                projected,
                _clock.GetUtcNow());
        }

        private static void ValidateProjectedConsequences(
            IReadOnlyList<ITruth> authoritativeTruths,
            IReadOnlyList<IConsequence> consequences)
        {
            if (authoritativeTruths == null)
            {
                throw new ArgumentNullException(
                    nameof(authoritativeTruths));
            }

            for (var consequenceIndex = 0;
                 consequenceIndex < consequences.Count;
                 consequenceIndex++)
            {
                var consequence =
                    consequences[consequenceIndex];

                if (consequence == null)
                {
                    throw new InvalidOperationException(
                        "The Consequence projection policy returned a null Consequence.");
                }

                if (consequence.Truths == null)
                {
                    throw new InvalidOperationException(
                        "A projected Consequence returned a null Truth collection.");
                }

                for (var truthIndex = 0;
                     truthIndex < consequence.Truths.Count;
                     truthIndex++)
                {
                    var truth =
                        consequence.Truths[truthIndex];

                    if (truth == null)
                    {
                        throw new InvalidOperationException(
                            "A projected Consequence contains a null Truth reference.");
                    }

                    if (!ContainsExactTruth(
                            authoritativeTruths,
                            truth))
                    {
                        throw new InvalidOperationException(
                            "A projected Consequence references a Truth " +
                            "outside the exact congealment result.");
                    }
                }
            }
        }

        private static bool ContainsExactTruth(
            IReadOnlyList<ITruth> truths,
            ITruth candidate)
        {
            for (var index = 0;
                 index < truths.Count;
                 index++)
            {
                if (ReferenceEquals(
                        truths[index],
                        candidate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}