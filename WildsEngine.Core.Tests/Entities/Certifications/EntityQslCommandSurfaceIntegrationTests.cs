using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Commands;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities
{
    /// <summary>
    /// Certifies the final Entity Surface shorthand/request seam.
    ///
    /// Entity Surfaces establish exact live bindings.
    /// Recognized shorthand prepares QSL runtime state.
    /// Ordinary QSL remains unchanged and enters the existing
    /// Wilds request lifecycle.
    /// </summary>
    public sealed class EntityQslCommandSurfaceIntegrationTests
    {
        [Fact]
        public async Task
            Entity_surface_shorthand_prepares_state_and_ordinary_qsl_enters_wilds()
        {
            TestLog.Section(
                "ENTITY SURFACE - QSL COMMAND/REQUEST INTEGRATION");

            TestLog.Test(
                "Entity Surface shorthand prepares state before " +
                "ordinary QSL enters the existing Wilds lifecycle.");

            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            var qslEngine =
                new QslEngine(
                    context);

            var commands =
                new QslCommandProcessor(
                    qslEngine,
                    context);

            var stateAdapter =
                new EntityQslStateAdapter(
                    qslEngine);

            var requestAdapter =
                new EntityQslRequestAdapter(
                    commands);

            var player =
                new TestPlayer(
                    50.0);

            var action =
                new TestAction(
                    10.0);

            var playerEntity =
                new TestEntity(
                    "player.entity");

            var actionEntity =
                new TestEntity(
                    "action.entity");

            var playerSurface =
                new TestEngineSurface(
                    playerEntity,
                    player,
                    "player",
                    "Player");

            var actionSurface =
                new TestEngineSurface(
                    actionEntity,
                    action,
                    "action",
                    "Action");

            var surfaces =
                new IEntitySurface[]
                {
                    playerSurface,
                    actionSurface
                };

            var attachedFacts =
                new Dictionary<
                    IEntitySurface,
                    IReadOnlyList<IFact>>();

            TestLog.Task(
                "Binding exact Entity Surface targets to QSL.");

            var playerBinding =
                stateAdapter.Bind(
                    playerSurface);

            var actionBinding =
                stateAdapter.Bind(
                    actionSurface);

            Assert.Same(
                player,
                playerBinding.Value);

            Assert.Same(
                action,
                actionBinding.Value);

            TestLog.State(
                "Player exact target bound = " +
                ReferenceEquals(
                    player,
                    playerBinding.Value));

            TestLog.State(
                "Action exact target bound = " +
                ReferenceEquals(
                    action,
                    actionBinding.Value));

            const string shorthand =
                "state-add action SpaceDustAppetite 87";

            TestLog.Task(
                "Submitting received shorthand through the " +
                "Entity request adapter.");

            var shorthandHandled =
                requestAdapter.TryCreateRequest(
                    shorthand,
                    surfaces,
                    attachedFacts,
                    out var shorthandRequest,
                    out var commandResult);

            Assert.True(
                shorthandHandled);

            Assert.Null(
                shorthandRequest);

            Assert.NotNull(
                commandResult);

            Assert.Equal(
                QslCommandKind.StateAdded,
                commandResult!.Kind);

            Assert.True(
                commandResult.Succeeded);

            Assert.Equal(
                87.0,
                Assert.IsType<double>(
                    commandResult.Value),
                10);

            TestLog.State(
                "Shorthand handled = " +
                shorthandHandled);

            TestLog.State(
                "Command kind = " +
                commandResult.Kind);

            TestLog.State(
                "action.SpaceDustAppetite = " +
                commandResult.Value);

            const string goldenSource =
                "player.Health = player.Health - " +
                "(action.Amount * " +
                "(action.SpaceDustAppetite / 100))";

            TestLog.Task(
                "Submitting ordinary QSL through the same " +
                "Entity request adapter.");

            var ordinaryHandled =
                requestAdapter.TryCreateRequest(
                    goldenSource,
                    surfaces,
                    attachedFacts,
                    out var request,
                    out var ordinaryCommandResult);

            Assert.False(
                ordinaryHandled);

            Assert.Null(
                ordinaryCommandResult);

            Assert.NotNull(
                request);

            Assert.Equal(
                goldenSource,
                request!.Source);

            Assert.Equal(
                2,
                request.Consumers.Count);

            Assert.Empty(
                request.StatePackages);

            Assert.Equal(
                50.0,
                player.Health,
                10);

            TestLog.State(
                "Ordinary QSL handled as shorthand = " +
                ordinaryHandled);

            TestLog.State(
                "Player.Health before Wilds = " +
                player.Health);

            var component =
                new TestComponent(
                    "entity.qsl.command-surface");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var moment =
                await executor.ExecuteAsync(
                    request);

            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Null(
                moment.Exception);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Equal(
                41.3,
                Convert.ToDouble(
                    moment.Result),
                10);

            Assert.Equal(
                41.3,
                player.Health,
                10);

            Assert.Single(
                wildsEngine.Timeline.Moments);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            Assert.Single(
                wildsEngine.Epoch.Timelines);

            Assert.Same(
                wildsEngine.Timeline,
                wildsEngine.Epoch.Timelines[0]);

            TestLog.State(
                "Moment outcome = " +
                moment.Outcome);

            TestLog.State(
                "Player.Health after Wilds = " +
                player.Health);

            TestLog.State(
                "Timeline Moments = " +
                wildsEngine.Timeline.Count);

            TestLog.State(
                "Epoch Timelines = " +
                wildsEngine.Epoch.Count);

            TestLog.Pass(
                "Entity Surface shorthand prepared runtime state, " +
                "ordinary QSL entered Wilds unchanged, and the " +
                "exact Player resolved from 50 to 41.3.");
        }

        [Fact]
        public async Task
            Entity_surface_removal_makes_runtime_vocabulary_unresolved_without_mutation()
        {
            TestLog.Section(
                "ENTITY SURFACE - QSL COMMAND/REQUEST REGRESSION");

            TestLog.Test(
                "Removed shorthand vocabulary fails honestly through Wilds without mutation.");

            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            var qslEngine =
                new QslEngine(
                    context);

            var stateAdapter =
                new EntityQslStateAdapter(
                    qslEngine);

            var requestAdapter =
                new EntityQslRequestAdapter(
                    new QslCommandProcessor(
                        qslEngine,
                        context));

            var player =
                new TestPlayer(
                    50.0);

            var action =
                new TestAction(
                    10.0);

            var playerSurface =
                new TestEngineSurface(
                    new TestEntity("player.entity"),
                    player,
                    "player",
                    "Player");

            var actionSurface =
                new TestEngineSurface(
                    new TestEntity("action.entity"),
                    action,
                    "action",
                    "Action");

            var surfaces =
                new IEntitySurface[]
                {
                    playerSurface,
                    actionSurface
                };

            var attachedFacts =
                new Dictionary<
                    IEntitySurface,
                    IReadOnlyList<IFact>>();

            stateAdapter.Bind(
                playerSurface);

            stateAdapter.Bind(
                actionSurface);

            Assert.True(
                requestAdapter.TryCreateRequest(
                    "state-add action SpaceDustAppetite 87",
                    surfaces,
                    attachedFacts,
                    out var addRequest,
                    out var addResult));

            Assert.Null(
                addRequest);

            Assert.NotNull(
                addResult);

            Assert.True(
                addResult!.Succeeded);

            Assert.True(
                requestAdapter.TryCreateRequest(
                    "state-remove action SpaceDustAppetite",
                    surfaces,
                    attachedFacts,
                    out var removeRequest,
                    out var removeResult));

            Assert.Null(
                removeRequest);

            Assert.NotNull(
                removeResult);

            Assert.Equal(
                QslCommandKind.StateRemoved,
                removeResult!.Kind);

            Assert.True(
                removeResult.Succeeded);

            const string goldenSource =
                "player.Health = player.Health - " +
                "(action.Amount * " +
                "(action.SpaceDustAppetite / 100))";

            Assert.False(
                requestAdapter.TryCreateRequest(
                    goldenSource,
                    surfaces,
                    attachedFacts,
                    out var request,
                    out var commandResult));

            Assert.Null(
                commandResult);

            Assert.NotNull(
                request);

            var component =
                new TestComponent(
                    "entity.qsl.command-surface.removal");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var moment =
                await executor.ExecuteAsync(
                    request!);

            Assert.Equal(
                MomentOutcome.Failed,
                moment.Outcome);

            Assert.NotNull(
                moment.Exception);

            Assert.Equal(
                50.0,
                player.Health,
                10);

            Assert.Single(
                wildsEngine.Timeline.Moments);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            TestLog.State(
                "SpaceDustAppetite after removal = unresolved");

            TestLog.State(
                "Moment outcome = " +
                moment.Outcome);

            TestLog.State(
                "Player.Health after failed Wilds processing = " +
                player.Health);

            TestLog.Pass(
                "Removed vocabulary failed through Wilds without mutating the exact Player.");
        }

        [Fact]
        public void
            Entity_surface_malformed_shorthand_is_rejected_without_creating_request()
        {
            TestLog.Section(
                "ENTITY SURFACE - MALFORMED SHORTHAND REGRESSION");

            TestLog.Test(
                "Malformed known shorthand is handled as a command error, not a Wilds QSL request.");

            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            var qslEngine =
                new QslEngine(
                    context);

            var requestAdapter =
                new EntityQslRequestAdapter(
                    new QslCommandProcessor(
                        qslEngine,
                        context));

            var handled =
                requestAdapter.TryCreateRequest(
                    "state-add",
                    Array.Empty<IEntitySurface>(),
                    new Dictionary<
                        IEntitySurface,
                        IReadOnlyList<IFact>>(),
                    out var request,
                    out var commandResult);

            Assert.True(
                handled);

            Assert.Null(
                request);

            Assert.NotNull(
                commandResult);

            Assert.Equal(
                QslCommandKind.Error,
                commandResult!.Kind);

            Assert.False(
                commandResult.Succeeded);

            Assert.Contains(
                "Usage: state-add",
                commandResult.Message,
                StringComparison.OrdinalIgnoreCase);

            TestLog.State(
                "Handled as shorthand = " +
                handled);

            TestLog.State(
                "Wilds request created = " +
                (request is not null));

            TestLog.State(
                "Command result = " +
                commandResult.Kind);

            TestLog.Pass(
                "Malformed shorthand stopped at the command surface and created no Wilds request.");
        }

        private static Core.WildsEngine
            CreateWildsEngine(
                IComponent component)
        {
            return new Core.WildsEngine(
                component,
                new WildsEngineOptions
                {
                    Layer =
                        "certification.entity.qsl.command-surface",
                    RetainedInstructionsProcessors =
                        1,
                    ConcurrentInstructionsProcessors =
                        1,
                    RetainedTaskProcessors =
                        1,
                    ConcurrentTaskProcessors =
                        1
                });
        }

        private sealed class TestPlayer
        {
            public TestPlayer(
                double health)
            {
                Health =
                    health;
            }

            public double Health { get; set; }
        }

        private sealed class TestAction
        {
            public TestAction(
                double amount)
            {
                Amount =
                    amount;
            }

            public double Amount { get; }
        }
    }
}
