using System;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Truths;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Truths
{
    public sealed class DiscoveryTruthProfile :
        IdentifierBase,
        IDiscoveryTruthProfile
    {
        public DiscoveryTruthProfile(
            Guid id,
            string slug,
            string version,
            string scope,
            ITruthCongealmentPolicy congealmentPolicy)
            : base(
                id,
                slug)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException(
                    "A Truth profile version is required.",
                    nameof(version));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException(
                    "A Truth profile scope is required.",
                    nameof(scope));
            }

            CongealmentPolicy =
                congealmentPolicy ??
                throw new ArgumentNullException(
                    nameof(congealmentPolicy));

            Version =
                version.Trim();

            Scope =
                scope.Trim();
        }

        public string Version { get; }

        public string Scope { get; }

        public ITruthCongealmentPolicy CongealmentPolicy
        {
            get;
        }
    }
}