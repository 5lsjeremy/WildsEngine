using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QSL.Core;
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
    /// Certifies the Entity Surface -> Wilds/QSL request boundary.
    ///
    /// Exact consumer-owned runtime target
    ///     -> Entity
    ///     -> Entity-owned Fact
    ///     -> Entity Surface
    ///     -> EntityQslRequestAdapter
    ///     -> WildsQslRequest
    ///     -> existing Wilds/QSL lifecycle
    ///     -> exact consumer-owned mutation
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    ///
    /// The adapter performs translation only.
    /// Wilds and QSL retain their already-certified
    /// responsibilities.
    /// </summary>
    public sealed class
        EntityQslRequestAdapterCertificationTests
    {
        [Fact]
        public async Task
            Entity_surface_reaches_existing_wilds_qsl_boundary()
        {
            TestLog.Section(
                "ENTITY GATE E1 - WILDS/QSL REQUEST BOUNDARY");

            TestLog.Test(
                "Entity-owned knowledge reaches the existing " +
                "Wilds/QSL request lifecycle.");

            /*
             * Exact consumer-owned runtime objects.
             */

            var player =
                new TestPlayer(
                    50.0);

            var action =
                new TestAction(
                    10.0);

            TestLog.State(
                $"Player native Health = {player.Health:R}");

            TestLog.State(
                $"Action native Amount = {action.Amount:R}");

            /*
             * Entity knowledge exists independently
             * of the consumer-owned runtime objects.
             */

            var playerEntity =
                new TestEntity(
                    "player.entity");

            var actionEntity =
                new TestEntity(
                    "action.entity");

            TestLog.State(
                $"Player Entity = " +
                $"{playerEntity.Slug} / {playerEntity.Id}");

            TestLog.State(
                $"Action Entity = " +
                $"{actionEntity.Slug} / {actionEntity.Id}");

            var spaceDustAppetite =
                new TestFact(
                    "action.space-dust-appetite",
                    actionEntity,
                    "SpaceDustAppetite",
                    87.0);

            /*
             * Establish authoritative Entity ownership.
             */

            TestLog.Task(
                "Registering SpaceDustAppetite as " +
                "Action Entity knowledge.");

            Assert.True(
                actionEntity.RegisterFact(
                    spaceDustAppetite));

            Assert.Empty(
                playerEntity.Facts);

            Assert.Single(
                actionEntity.Facts);

            Assert.Same(
                spaceDustAppetite,
                actionEntity.Facts[0]);

            Assert.Same(
                actionEntity,
                actionEntity.Facts[0].Entity);

            TestLog.State(
                $"Fact slug = {spaceDustAppetite.Slug}");

            TestLog.State(
                $"Fact path = {spaceDustAppetite.Path}");

            TestLog.State(
                $"Fact value = {spaceDustAppetite.Value}");

            TestLog.State(
                $"Action Entity Fact count = " +
                $"{actionEntity.Facts.Count}");

            TestLog.State(
                $"Exact Fact reference owned = " +
                $"{ReferenceEquals(
                    spaceDustAppetite,
                    actionEntity.Facts[0])}");

            TestLog.Pass(
                "Exact Fact is owned by the Action Entity.");

            /*
             * Surface establishes controlled exposure.
             */

            TestLog.Task(
                "Creating controlled Entity Surfaces.");

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

            TestLog.State(
                $"Player Surface = " +
                $"{playerSurface.Alias} / " +
                $"{playerSurface.ConsumerType}");

            TestLog.State(
                $"Action Surface = " +
                $"{actionSurface.Alias} / " +
                $"{actionSurface.ConsumerType}");

            TestLog.State(
                $"Player Surface exact Entity = " +
                $"{ReferenceEquals(
                    playerEntity,
                    playerSurface.Entity)}");

            TestLog.State(
                $"Action Surface exact Entity = " +
                $"{ReferenceEquals(
                    actionEntity,
                    actionSurface.Entity)}");

            TestLog.State(
                $"Player Surface exact target = " +
                $"{ReferenceEquals(
                    player,
                    playerSurface.Target)}");

            TestLog.State(
                $"Action Surface exact target = " +
                $"{ReferenceEquals(
                    action,
                    actionSurface.Target)}");

            var adapter =
                new EntityQslRequestAdapter();

            const string source =
                "player.Health = player.Health - " +
                "(action.Amount * " +
                "(action.SpaceDustAppetite / 100))";

            TestLog.Task(
                "Translating Entity Surfaces and Facts " +
                "into WildsQslRequest.");

            var request =
                adapter.CreateRequest(
                    source,
                    new IEntitySurface[]
                    {
                        playerSurface,
                        actionSurface
                    },
                    new Dictionary<
                        IEntitySurface,
                        IReadOnlyList<IFact>>
                    {
                        {
                            actionSurface,
                            new IFact[]
                            {
                                spaceDustAppetite
                            }
                        }
                    });

            /*
             * Gate E1-A:
             * Entity -> Surface -> request translation.
             */

            TestLog.Section(
                "ENTITY GATE E1-A - REQUEST TRANSLATION");

            Assert.Equal(
                2,
                request.Consumers.Count);

            Assert.Single(
                request.StatePackages);

            Assert.Equal(
                "player",
                request.Consumers[0].Name);

            Assert.Equal(
                "action",
                request.Consumers[1].Name);

            Assert.Same(
                player,
                request.Consumers[0].Value);

            Assert.Same(
                action,
                request.Consumers[1].Value);

            TestLog.State(
                $"Consumer bindings = " +
                $"{request.Consumers.Count}");

            TestLog.State(
                $"State packages = " +
                $"{request.StatePackages.Count}");

            TestLog.State(
                $"Consumer[0] = " +
                $"{request.Consumers[0].Name}");

            TestLog.State(
                $"Consumer[0] exact Player target = " +
                $"{ReferenceEquals(
                    player,
                    request.Consumers[0].Value)}");

            TestLog.State(
                $"Consumer[1] = " +
                $"{request.Consumers[1].Name}");

            TestLog.State(
                $"Consumer[1] exact Action target = " +
                $"{ReferenceEquals(
                    action,
                    request.Consumers[1].Value)}");

            /*
             * Surface retains exact targets and Entities.
             */

            Assert.Same(
                player,
                playerSurface.Target);

            Assert.Same(
                action,
                actionSurface.Target);

            Assert.Same(
                playerEntity,
                playerSurface.Entity);

            Assert.Same(
                actionEntity,
                actionSurface.Entity);

            /*
             * Fact provenance remains exact.
             */

            Assert.Same(
                actionEntity,
                spaceDustAppetite.Entity);

            Assert.Same(
                spaceDustAppetite,
                actionEntity.Facts[0]);

            Assert.Equal(
                "SpaceDustAppetite",
                spaceDustAppetite.Path);

            Assert.Equal(
                87.0,
                Assert.IsType<double>(
                    spaceDustAppetite.Value));

            Assert.Same(
                action,
                request.StatePackages[0].Target);

            Assert.Same(
                actionSurface.Target,
                request.StatePackages[0].Target);

            TestLog.State(
                $"State package exact Action target = " +
                $"{ReferenceEquals(
                    action,
                    request.StatePackages[0].Target)}");

            TestLog.State(
                $"Entity Fact path = " +
                $"{spaceDustAppetite.Path}");

            TestLog.State(
                $"Entity Fact value = " +
                $"{spaceDustAppetite.Value}");

            TestLog.Pass(
                "Entity data translated into the existing " +
                "Wilds/QSL request contract.");

            /*
             * Gate E1-B:
             * Existing certified Wilds/QSL execution.
             */

            TestLog.Section(
                "ENTITY GATE E1-B - EXISTING CERTIFIED PIPELINE");

            var qslEngine =
                CreateQslEngine();

            var component =
                new TestComponent(
                    "entity.qsl.certification");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            Assert.Equal(
                0,
                wildsEngine.Timeline.Count);

            TestLog.State(
                "QSL source:");

            TestLog.Write(
                "    " + source);

            TestLog.State(
                "Consumer input:");

            TestLog.Write(
                $"    player.Health = {player.Health:R}");

            TestLog.Write(
                $"    action.Amount = {action.Amount:R}");

            TestLog.Write(
                $"    action.SpaceDustAppetite = " +
                $"{spaceDustAppetite.Value}");

            TestLog.State(
                $"Timeline Moments before execution = " +
                $"{wildsEngine.Timeline.Count}");

            TestLog.Task(
                "Submitting Entity-generated WildsQslRequest.");

            var moment =
                await executor.ExecuteAsync(
                        request)
                    .ConfigureAwait(false);

            TestLog.Task(
                "Wilds/QSL processing returned.");

            TestLog.State(
                $"Moment outcome = {moment.Outcome}");

            TestLog.State(
                $"Pulse = {moment.PulseId}");

            TestLog.State(
                $"WorkUnits = {moment.WorkUnitIds.Count}");

            TestLog.State(
                $"Moment result = {moment.Result}");

            TestLog.State(
                $"Player.Health after processing = " +
                $"{player.Health:R}");

            TestLog.State(
                $"Timeline Moments = " +
                $"{wildsEngine.Timeline.Count}");

            TestLog.State(
                $"Epoch Timelines = " +
                $"{wildsEngine.Epoch.Count}");

            /*
             * Golden QSL behavior:
             *
             * 50 - (10 * (87 / 100))
             * = 41.3
             */

            TestLog.Expected(
                "Player.Health should resolve from 50 to 41.3.");

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

            TestLog.Pass(
                "Exact consumer-owned Player mutated " +
                "from 50 to 41.3.");

            /*
             * Exact consumer identity remains intact.
             */

            TestLog.Task(
                "Verifying consumer identity continuity.");

            Assert.Same(
                player,
                playerSurface.Target);

            Assert.Same(
                action,
                actionSurface.Target);

            Assert.Same(
                player,
                request.Consumers[0].Value);

            Assert.Same(
                action,
                request.Consumers[1].Value);

            Assert.Same(
                action,
                request.StatePackages[0].Target);

            TestLog.State(
                $"Player identity survived = " +
                $"{ReferenceEquals(
                    player,
                    request.Consumers[0].Value)}");

            TestLog.State(
                $"Action identity survived = " +
                $"{ReferenceEquals(
                    action,
                    request.Consumers[1].Value)}");

            TestLog.Pass(
                "Exact consumer-owned runtime objects survived.");

            /*
             * Entity and Fact provenance remain intact.
             */

            TestLog.Task(
                "Verifying Entity and Fact provenance.");

            Assert.Empty(
                playerEntity.Facts);

            Assert.Single(
                actionEntity.Facts);

            Assert.Same(
                spaceDustAppetite,
                actionEntity.Facts[0]);

            Assert.Same(
                actionEntity,
                spaceDustAppetite.Entity);

            Assert.Equal(
                "SpaceDustAppetite",
                spaceDustAppetite.Path);

            Assert.Equal(
                87.0,
                Assert.IsType<double>(
                    spaceDustAppetite.Value));

            TestLog.State(
                $"Action Entity Fact count after processing = " +
                $"{actionEntity.Facts.Count}");

            TestLog.State(
                $"Fact path after processing = " +
                $"{spaceDustAppetite.Path}");

            TestLog.State(
                $"Fact value after processing = " +
                $"{spaceDustAppetite.Value}");

            TestLog.Pass(
                "Entity and Fact provenance remained intact.");

            /*
             * Existing Wilds history remains authoritative.
             */

            TestLog.Task(
                "Verifying Wilds temporal history.");

            Assert.Single(
                wildsEngine.Timeline.Moments);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            Assert.Equal(
                1,
                wildsEngine.Epoch.Count);

            Assert.Single(
                wildsEngine.Epoch.Timelines);

            Assert.Same(
                wildsEngine.Timeline,
                wildsEngine.Epoch.Timelines[0]);

            Assert.Single(
                wildsEngine.Epoch
                    .Timelines[0]
                    .Moments);

            Assert.Same(
                moment,
                wildsEngine.Epoch
                    .Timelines[0]
                    .Moments[0]);

            TestLog.State(
                $"Exact Moment retained by Timeline = " +
                $"{ReferenceEquals(
                    moment,
                    wildsEngine.Timeline.Moments[0])}");

            TestLog.State(
                $"Exact Timeline retained by Epoch = " +
                $"{ReferenceEquals(
                    wildsEngine.Timeline,
                    wildsEngine.Epoch.Timelines[0])}");

            TestLog.Pass(
                "Existing Wilds temporal history remained authoritative.");

            TestLog.Pass(
                "ENTITY GATE E1 CERTIFIED.");
        }

        [Fact]
        public void
            Surface_without_attached_facts_creates_binding_without_package()
        {
            TestLog.Section(
                "ENTITY GATE E1 - NATIVE CONSUMER ONLY");

            TestLog.Test(
                "Surface creates a consumer binding without " +
                "inventing an attached-state package.");

            var player =
                new TestPlayer(
                    50.0);

            var playerEntity =
                new TestEntity(
                    "player.entity");

            var playerSurface =
                new TestEngineSurface(
                    playerEntity,
                    player,
                    "player",
                    "Player");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Task(
                "Creating request without attached Entity Facts.");

            var request =
                adapter.CreateRequest(
                    "player.Health",
                    new IEntitySurface[]
                    {
                        playerSurface
                    },
                    new Dictionary<
                        IEntitySurface,
                        IReadOnlyList<IFact>>());

            Assert.Single(
                request.Consumers);

            Assert.Empty(
                request.StatePackages);

            Assert.Equal(
                "player",
                request.Consumers[0].Name);

            Assert.Same(
                player,
                request.Consumers[0].Value);

            Assert.Same(
                playerEntity,
                playerSurface.Entity);

            Assert.Same(
                player,
                playerSurface.Target);

            TestLog.State(
                $"Consumer bindings = {request.Consumers.Count}");

            TestLog.State(
                $"State packages = {request.StatePackages.Count}");

            TestLog.State(
                $"Exact Player target = " +
                $"{ReferenceEquals(
                    player,
                    request.Consumers[0].Value)}");

            TestLog.Pass(
                "Native consumer binding required no attached-state package.");
        }

        [Fact]
        public void
            Adapter_rejects_fact_owned_by_another_entity()
        {
            TestLog.Section(
                "ENTITY GATE E1 - FOREIGN FACT REJECTION");

            TestLog.Test(
                "Adapter rejects a Fact owned by another Entity.");

            var player =
                new TestPlayer(
                    50.0);

            var playerEntity =
                new TestEntity(
                    "player.entity");

            var foreignEntity =
                new TestEntity(
                    "foreign.entity");

            var foreignFact =
                new TestFact(
                    "foreign.curiosity",
                    foreignEntity,
                    "Curiosity",
                    11.0);

            Assert.True(
                foreignEntity.RegisterFact(
                    foreignFact));

            Assert.Single(
                foreignEntity.Facts);

            Assert.Empty(
                playerEntity.Facts);

            var playerSurface =
                new TestEngineSurface(
                    playerEntity,
                    player,
                    "player",
                    "Player");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Task(
                "Attempting to attach foreign Entity Fact.");

            TestLog.Expected(
                "ArgumentException");

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        adapter.CreateRequest(
                            "player.Health",
                            new IEntitySurface[]
                            {
                                playerSurface
                            },
                            new Dictionary<
                                IEntitySurface,
                                IReadOnlyList<IFact>>
                            {
                                {
                                    playerSurface,
                                    new IFact[]
                                    {
                                        foreignFact
                                    }
                                }
                            }));

            TestLog.State(
                $"Rejected with = " +
                $"{exception.GetType().Name}");

            /*
             * Rejection must not mutate either Entity.
             */

            Assert.Empty(
                playerEntity.Facts);

            Assert.Single(
                foreignEntity.Facts);

            Assert.Same(
                foreignFact,
                foreignEntity.Facts[0]);

            TestLog.Pass(
                "Foreign Fact was rejected without mutating Entity ownership.");
        }

        [Fact]
        public void
            Adapter_rejects_empty_qsl_source()
        {
            TestLog.Section(
                "ENTITY GATE E1 - EMPTY SOURCE");

            TestLog.Test(
                "Adapter rejects empty QSL source.");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Expected(
                "ArgumentException");

            Assert.Throws<ArgumentException>(
                () =>
                    adapter.CreateRequest(
                        " ",
                        Array.Empty<IEntitySurface>(),
                        new Dictionary<
                            IEntitySurface,
                            IReadOnlyList<IFact>>()));

            TestLog.Pass(
                "Empty QSL source was rejected.");
        }

        [Fact]
        public void
            Adapter_rejects_null_surfaces()
        {
            TestLog.Section(
                "ENTITY GATE E1 - NULL SURFACES");

            TestLog.Test(
                "Adapter rejects a null Surface collection.");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Expected(
                "ArgumentNullException");

            Assert.Throws<ArgumentNullException>(
                () =>
                    adapter.CreateRequest(
                        "1 + 2",
                        null!,
                        new Dictionary<
                            IEntitySurface,
                            IReadOnlyList<IFact>>()));

            TestLog.Pass(
                "Null Surface collection was rejected.");
        }

        [Fact]
        public void
            Adapter_rejects_null_attached_fact_map()
        {
            TestLog.Section(
                "ENTITY GATE E1 - NULL FACT MAP");

            TestLog.Test(
                "Adapter rejects a null attached-Fact map.");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Expected(
                "ArgumentNullException");

            Assert.Throws<ArgumentNullException>(
                () =>
                    adapter.CreateRequest(
                        "1 + 2",
                        Array.Empty<IEntitySurface>(),
                        null!));

            TestLog.Pass(
                "Null attached-Fact map was rejected.");
        }

        [Fact]
        public void
            Adapter_rejects_null_surface_entry()
        {
            TestLog.Section(
                "ENTITY GATE E1 - NULL SURFACE ENTRY");

            TestLog.Test(
                "Adapter rejects a null Surface entry.");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Expected(
                "ArgumentException");

            Assert.Throws<ArgumentException>(
                () =>
                    adapter.CreateRequest(
                        "1 + 2",
                        new IEntitySurface[]
                        {
                            null!
                        },
                        new Dictionary<
                            IEntitySurface,
                            IReadOnlyList<IFact>>()));

            TestLog.Pass(
                "Null Surface entry was rejected.");
        }

        [Fact]
        public void
            Adapter_rejects_null_attached_fact_entry()
        {
            TestLog.Section(
                "ENTITY GATE E1 - NULL FACT ENTRY");

            TestLog.Test(
                "Adapter rejects a null attached Fact entry.");

            var target =
                new TestPlayer(
                    50.0);

            var entity =
                new TestEntity(
                    "player.entity");

            var surface =
                new TestEngineSurface(
                    entity,
                    target,
                    "player",
                    "Player");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Expected(
                "ArgumentException");

            Assert.Throws<ArgumentException>(
                () =>
                    adapter.CreateRequest(
                        "player.Health",
                        new IEntitySurface[]
                        {
                            surface
                        },
                        new Dictionary<
                            IEntitySurface,
                            IReadOnlyList<IFact>>
                        {
                            {
                                surface,
                                new IFact[]
                                {
                                    null!
                                }
                            }
                        }));

            TestLog.Pass(
                "Null attached Fact entry was rejected.");
        }

        [Fact]
        public void
            Adapter_rejects_duplicate_attached_fact_paths()
        {
            TestLog.Section(
                "ENTITY GATE E1 - DUPLICATE VOCABULARY");

            TestLog.Test(
                "Adapter rejects duplicate attached Fact paths.");

            var action =
                new TestAction(
                    10.0);

            var actionEntity =
                new TestEntity(
                    "action.entity");

            var first =
                new TestFact(
                    "action.appetite.first",
                    actionEntity,
                    "SpaceDustAppetite",
                    87.0);

            var second =
                new TestFact(
                    "action.appetite.second",
                    actionEntity,
                    "SpaceDustAppetite",
                    25.0);

            Assert.True(
                actionEntity.RegisterFact(
                    first));

            Assert.True(
                actionEntity.RegisterFact(
                    second));

            Assert.Equal(
                2,
                actionEntity.Facts.Count);

            TestLog.State(
                $"Action Entity Fact count = " +
                $"{actionEntity.Facts.Count}");

            TestLog.State(
                $"First path = {first.Path} / " +
                $"value = {first.Value}");

            TestLog.State(
                $"Second path = {second.Path} / " +
                $"value = {second.Value}");

            var actionSurface =
                new TestEngineSurface(
                    actionEntity,
                    action,
                    "action",
                    "Action");

            var adapter =
                new EntityQslRequestAdapter();

            TestLog.Task(
                "Attempting to expose duplicate attached vocabulary.");

            TestLog.Expected(
                "ArgumentException");

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        adapter.CreateRequest(
                            "action.SpaceDustAppetite",
                            new IEntitySurface[]
                            {
                                actionSurface
                            },
                            new Dictionary<
                                IEntitySurface,
                                IReadOnlyList<IFact>>
                            {
                                {
                                    actionSurface,
                                    new IFact[]
                                    {
                                        first,
                                        second
                                    }
                                }
                            }));

            TestLog.State(
                $"Rejected with = " +
                $"{exception.GetType().Name}");

            TestLog.Pass(
                "Duplicate attached vocabulary was rejected.");
        }

        private static QslEngine CreateQslEngine()
        {
            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            return new QslEngine(
                context);
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
                        "certification.entity.qsl",

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