using System;
using System.Collections.Generic;
using QSL.Core.Commands;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Translates Entity Surfaces and approved Facts into the
    /// existing Wilds/QSL request contract.
    ///
    /// When configured with QslCommandProcessor, the adapter can
    /// first attempt to process recognized host shorthand. Input
    /// that is not shorthand remains unchanged and is translated
    /// into WildsQslRequest for the existing Wilds lifecycle.
    /// </summary>
    public sealed class EntityQslRequestAdapter
    {
        private readonly QslCommandProcessor? _commands;

        /// <summary>
        /// Creates a translation-only adapter.
        ///
        /// This constructor preserves the existing CreateRequest
        /// surface and does not enable shorthand processing.
        /// </summary>
        public EntityQslRequestAdapter()
        {
        }

        /// <summary>
        /// Creates an adapter that can discriminate and process
        /// recognized QSL host shorthand before request creation.
        /// </summary>
        public EntityQslRequestAdapter(
            QslCommandProcessor commands)
        {
            _commands =
                commands ??
                throw new ArgumentNullException(
                    nameof(commands));
        }

        public QslCommandProcessor? Commands =>
            _commands;

        /// <summary>
        /// Attempts to process source through the Entity-facing
        /// command/request surface.
        ///
        /// Returns true when recognized shorthand was processed.
        /// In that case commandResult is populated and request is
        /// null.
        ///
        /// Returns false when source is ordinary QSL. In that case
        /// source is left unchanged, request is populated, and
        /// commandResult is null.
        ///
        /// Entity Surfaces must already be bound to the same QSL
        /// runtime before target-based shorthand is submitted.
        /// EntityQslStateAdapter owns that live binding lifecycle.
        /// </summary>
        public bool TryCreateRequest(
            string source,
            IReadOnlyList<IEntitySurface> surfaces,
            IReadOnlyDictionary<
                IEntitySurface,
                IReadOnlyList<IFact>> attachedFacts,
            out WildsQslRequest? request,
            out QslCommandResult? commandResult)
        {
            if (_commands == null)
            {
                throw new InvalidOperationException(
                    "This adapter was not configured with a " +
                    "QslCommandProcessor.");
            }

            ValidateInputs(
                source,
                surfaces,
                attachedFacts);

            if (_commands.TryExecuteCommand(
                    source,
                    out commandResult))
            {
                request =
                    null;

                return true;
            }

            commandResult =
                null;

            request =
                CreateRequest(
                    source,
                    surfaces,
                    attachedFacts);

            return false;
        }

        public WildsQslRequest CreateRequest(
            string source,
            IReadOnlyList<IEntitySurface> surfaces,
            IReadOnlyDictionary<
                IEntitySurface,
                IReadOnlyList<IFact>> attachedFacts)
        {
            ValidateInputs(
                source,
                surfaces,
                attachedFacts);

            var consumers =
                new List<QslConsumerBinding>(
                    surfaces.Count);

            var packages =
                new List<QslStatePackage>();

            for (var index = 0;
                 index < surfaces.Count;
                 index++)
            {
                var surface =
                    surfaces[index] ??
                    throw new ArgumentException(
                        "Surfaces cannot contain null.",
                        nameof(surfaces));

                consumers.Add(
                    new QslConsumerBinding(
                        surface.Alias,
                        surface.ConsumerType,
                        surface.Target));

                if (!attachedFacts.TryGetValue(
                        surface,
                        out var facts) ||
                    facts.Count == 0)
                {
                    continue;
                }

                var properties =
                    new Dictionary<string, object?>(
                        StringComparer.Ordinal);

                for (var factIndex = 0;
                     factIndex < facts.Count;
                     factIndex++)
                {
                    var fact =
                        facts[factIndex] ??
                        throw new ArgumentException(
                            "Attached Facts cannot contain null.",
                            nameof(attachedFacts));

                    if (!ReferenceEquals(
                            fact.Entity,
                            surface.Entity))
                    {
                        throw new ArgumentException(
                            "Attached Facts must belong to the " +
                            "exact Surface Entity.",
                            nameof(attachedFacts));
                    }

                    properties.Add(
                        fact.Name,
                        fact.Value);
                    
                    if (!fact.IsProven)
                    {
                        throw new InvalidOperationException(
                            "Only proven Facts can enter attached QSL state.");
                    }
                }

                packages.Add(
                    new QslStatePackage(
                        surface.Target,
                        properties));
            }

            return new WildsQslRequest(
                source.Trim(),
                consumers,
                packages);
        }

        private static void ValidateInputs(
            string source,
            IReadOnlyList<IEntitySurface> surfaces,
            IReadOnlyDictionary<
                IEntitySurface,
                IReadOnlyList<IFact>> attachedFacts)
        {
            if (string.IsNullOrWhiteSpace(
                    source))
            {
                throw new ArgumentException(
                    "QSL source cannot be empty.",
                    nameof(source));
            }

            if (surfaces == null)
            {
                throw new ArgumentNullException(
                    nameof(surfaces));
            }

            if (attachedFacts == null)
            {
                throw new ArgumentNullException(
                    nameof(attachedFacts));
            }
        }
    }
}
