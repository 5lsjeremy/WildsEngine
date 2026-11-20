using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Consequences;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Truths;

namespace WildsEngine.Discoveries.Consequences
{
    /// <summary>
    /// Owns the consumer-facing Consequence projection boundary.
    ///
    /// Consequences are derived only from Truth that has actually
    /// congealed. This surface does not create Truth, reinterpret
    /// Truth, or execute the Consequences it produces.
    ///
    /// A successful Truth congealment may legitimately project zero
    /// Consequences when the consumer projection policy determines
    /// that nothing follows from the Truth.
    ///
    /// A refused Truth congealment is different: there is no
    /// authoritative Truth from which Consequences may be projected,
    /// so consumer projection is not invoked.
    /// </summary>
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

        /// <summary>
        /// Registers one consumer-owned Consequence projection profile.
        ///
        /// The policy must expose stable identity so the registration
        /// can preserve exactly which consumer policy produced any
        /// later projection.
        /// </summary>
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
                    "The Consequence projection policy must expose " +
                    "stable identity.",
                    nameof(profile));
            }

            if (identifiedPolicy.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "The Consequence projection policy requires " +
                    "a non-empty identity.",
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

        /// <summary>
        /// Projects zero or more Consequences from an authoritative,
        /// successfully congealed Truth result.
        ///
        /// Important boundary:
        ///
        /// Congealed Truth -> consumer projection may run.
        ///
        /// Refused Truth -> projection stops here and the consumer
        /// projection policy is never invoked.
        ///
        /// An empty projection from successfully congealed Truth is
        /// valid. It means the consumer considered the Truth and
        /// determined that no Consequence follows from it.
        /// </summary>
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
                    "No Consequence profile exists for registered " +
                    "profile ID: " +
                    registration.ProfileId);
            }

            // Truth and Consequence remain scoped.
            //
            // A Consequence profile cannot consume Truth produced
            // under another registered scope.
            if (!string.Equals(
                    truthResult.Registration.Scope,
                    registration.Scope,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Truth and Consequence registration scopes must match.");
            }

            // D3 - TRUTH / CONSEQUENCE BOUNDARY
            //
            // This is deliberately NOT the same thing as:
            //
            //     truthResult.Truths.Count == 0
            //
            // Congealed tells us whether the Truth stage actually
            // authorized a Truth result.
            //
            // If congealment was refused, there is no authoritative
            // Truth from which Consequences are allowed to derive.
            // Therefore the consumer projection policy must never run.
            //
            // This keeps the semantic chain honest:
            //
            //     refused Truth -> STOP
            //
            // while still permitting:
            //
            //     congealed Truth
            //         -> projection policy runs
            //         -> zero Consequences is valid
            //
            if (!truthResult.Congealed)
            {
                throw new InvalidOperationException(
                    "Consequences cannot be projected from a Truth " +
                    "result that did not congeal.");
            }

            // From this point forward Truth has successfully
            // congealed, so the consumer is allowed to decide what,
            // if anything, follows from it.
            var projected =
                profile.ProjectionPolicy.Project(
                    truthResult.Truths);

            if (projected == null)
            {
                throw new InvalidOperationException(
                    "The Consequence projection policy returned null.");
            }

            // Consumer projection is flexible, but attribution is not.
            //
            // Every Truth referenced by every projected Consequence
            // must be one of the exact Truth instances supplied by
            // this congealment result.
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

        /// <summary>
        /// Ensures that consumer-projected Consequences remain
        /// attributable to the exact authoritative Truths supplied
        /// by the congealment result.
        ///
        /// The consumer may decide what a Consequence means, but it
        /// cannot silently substitute unrelated Truth.
        /// </summary>
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
                        "The Consequence projection policy returned " +
                        "a null Consequence.");
                }

                if (consequence.Truths == null)
                {
                    throw new InvalidOperationException(
                        "A projected Consequence returned a null " +
                        "Truth collection.");
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
                            "A projected Consequence contains a null " +
                            "Truth reference.");
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

        /// <summary>
        /// Uses exact reference identity intentionally.
        ///
        /// A semantically similar or reconstructed Truth is not the
        /// authoritative Truth supplied to this projection.
        /// </summary>
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