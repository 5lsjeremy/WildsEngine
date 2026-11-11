using System;
using System.Globalization;
using QSL.Core.Commands;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Tests.Logging;
using Xunit;

namespace QSL.Core.Tests.Commands
{
    /// <summary>
    /// Certifies the reusable QSL command surface.
    ///
    /// Execute processes recognized shorthand and evaluates
    /// all other input as ordinary QSL.
    ///
    /// TryExecuteCommand processes recognized shorthand while
    /// leaving ordinary QSL untouched for downstream hosts such
    /// as Wilds.
    ///
    /// Native bound-object properties are immediately available
    /// to ordinary QSL. Runtime-attached properties become
    /// available only after successful state-add processing.
    ///
    /// Properties that are neither native nor attached remain
    /// unresolved. Expressions depending on them must fail
    /// without mutating consumer-owned state.
    /// </summary>
    public sealed class QslCommandSurfaceParityCertificationTests
    {
        [Fact]
        public void Ordinary_qsl_falls_through_to_engine_evaluation()
        {
            TestLog.Section("QSL COMMAND SURFACE - ORDINARY EVALUATION");
            TestLog.Test("Execute routes ordinary QSL to QslEngine.");

            var harness = CreateHarness();
            const string source = "1 + 2";

            TestLog.State("Source: " + source);
            var result = harness.Processor.Execute(source);
            TestLog.State("Kind: " + result.Kind);
            TestLog.State("Succeeded: " + result.Succeeded);
            TestLog.State("Value: " + Format(result.Value));

            AssertSuccessfulEvaluation(result, 3.0);
            TestLog.Pass("Ordinary QSL evaluated through the command surface.");
        }

        [Fact]
        public void Try_execute_command_processes_recognized_state_shorthand()
        {
            TestLog.Section("QSL COMMAND SURFACE - SHORTHAND");
            TestLog.Test("Recognized shorthand updates exact bound runtime state.");

            var harness = CreateHarness();
            const string source = "state-add machine Heat 20";

            TestLog.State("Source: " + source);
            var handled = harness.Processor.TryExecuteCommand(source, out var result);
            LogCommandResult(handled, result);

            Assert.True(handled);
            Assert.NotNull(result);
            Assert.Equal(QslCommandKind.StateAdded, result!.Kind);
            Assert.True(result.Succeeded);
            Assert.Equal(20.0, Assert.IsType<double>(result.Value), 10);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);
            Assert.Same(harness.Machine, harness.MachineBinding.Value);

            TestLog.State("machine.Heat: 20");
            TestLog.Pass("Recognized shorthand updated the exact bound Machine.");
        }

        [Fact]
        public void Try_execute_command_leaves_ordinary_qsl_untouched()
        {
            TestLog.Section("QSL COMMAND SURFACE - DOWNSTREAM QSL");
            TestLog.Test("Ordinary QSL remains untouched for downstream Wilds processing.");

            var harness = CreateHarness();
            const string source = "player.Health = player.Health - 10";

            TestLog.State("Source: " + source);
            TestLog.State("Player.Health before: " + harness.Player.Health);

            var handled = harness.Processor.TryExecuteCommand(source, out var result);
            LogCommandResult(handled, result);

            Assert.False(handled);
            Assert.Null(result);
            Assert.Equal(50.0, harness.Player.Health, 10);
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);

            TestLog.State("Player.Health after discrimination: " + harness.Player.Health);
            TestLog.Pass("Ordinary QSL remained untouched for downstream processing.");
        }

        [Fact]
        public void Try_execute_command_processes_state_removal()
        {
            TestLog.Section("QSL COMMAND SURFACE - STATE REMOVAL");
            TestLog.Test("Recognized removal shorthand retires attached vocabulary.");

            var harness = CreateHarness();

            var addHandled = harness.Processor.TryExecuteCommand(
                "state-add machine Heat 20",
                out var addResult);

            Assert.True(addHandled);
            Assert.NotNull(addResult);
            Assert.True(addResult!.Succeeded);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);
            TestLog.State("machine.Heat after add: 20");

            var removeHandled = harness.Processor.TryExecuteCommand(
                "state-remove machine Heat",
                out var removeResult);

            LogCommandResult(removeHandled, removeResult);
            Assert.True(removeHandled);
            Assert.NotNull(removeResult);
            Assert.Equal(QslCommandKind.StateRemoved, removeResult!.Kind);
            Assert.True(removeResult.Succeeded);
            AssertFailedEvaluation(harness.Processor.Execute("machine.Heat"));
            Assert.Equal(50.0, harness.Player.Health, 10);

            TestLog.State("machine.Heat after removal: unresolved");
            TestLog.Pass("Removal shorthand retired attached vocabulary without native mutation.");
        }

        [Fact]
        public void Native_bound_object_properties_are_immediately_available()
        {
            TestLog.Section("QSL COMMAND SURFACE - NATIVE STATE");
            TestLog.Test("Native state is readable and mutates only by assignment.");

            var harness = CreateHarness();
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health - 10"), 40.0);
            Assert.Equal(50.0, harness.Player.Health, 10);

            var assigned = harness.Processor.Execute(
                "player.Health = player.Health - 10");

            AssertSuccessfulEvaluation(assigned, 40.0);
            Assert.Equal(40.0, harness.Player.Health, 10);
            Assert.Same(harness.Player, harness.PlayerBinding.Value);

            TestLog.State("Player.Health: 50 -> " + harness.Player.Health);
            TestLog.Pass("Native state remained exact and assignment-controlled.");
        }

        [Fact]
        public void Missing_runtime_vocabulary_fails_without_mutating_native_state()
        {
            TestLog.Section("QSL COMMAND SURFACE - FAILURE HONESTY");
            TestLog.Test("Missing vocabulary fails without committing assignment.");

            var harness = CreateHarness();
            var missing = harness.Processor.Execute("machine.Heat");
            AssertFailedEvaluation(missing);
            LogEvaluationFailure("machine.Heat", missing);

            var assignment = harness.Processor.Execute(
                "player.Health = player.Health - machine.Heat");

            AssertFailedEvaluation(assignment);
            Assert.Equal(50.0, harness.Player.Health, 10);
            LogEvaluationFailure("dependent assignment", assignment);

            TestLog.State("Player.Health after failed assignment: " + harness.Player.Health);
            TestLog.Pass("Missing vocabulary produced no native-state mutation.");
        }

        [Fact]
        public void State_add_introduces_runtime_vocabulary_for_exact_bound_target()
        {
            TestLog.Section("QSL COMMAND SURFACE - RUNTIME VOCABULARY");
            TestLog.Test("state-add introduces vocabulary on the exact target.");

            var harness = CreateHarness();
            AssertFailedEvaluation(harness.Processor.Execute("machine.Heat"));

            var added = harness.Processor.Execute("state-add machine Heat 20");
            Assert.Equal(QslCommandKind.StateAdded, added.Kind);
            Assert.True(added.Succeeded);
            Assert.Equal(20.0, Assert.IsType<double>(added.Value), 10);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);
            AssertSuccessfulEvaluation(
                harness.Processor.Execute("player.Health - machine.Heat"),
                30.0);
            Assert.Equal(50.0, harness.Player.Health, 10);
            Assert.Same(harness.Machine, harness.MachineBinding.Value);

            TestLog.State("machine.Heat: unresolved -> 20");
            TestLog.State("Player.Health remained: " + harness.Player.Health);
            TestLog.Pass("Runtime vocabulary composed without implicit mutation.");
        }

        [Theory]
        [InlineData(1.0, 30.0)]
        [InlineData(2.0, 10.0)]
        [InlineData(3.0, -10.0)]
        public void Runtime_heat_and_burn_level_compose_with_native_health(
            double burnLevel,
            double expectedHealth)
        {
            TestLog.Section("QSL COMMAND SURFACE - RUNTIME COMPOSITION");
            TestLog.Test("Heat and BurnLevel compose with native Health.");

            var harness = CreateHarness();
            Assert.True(harness.Processor.Execute("state-add machine Heat 20").Succeeded);
            Assert.True(harness.Processor.Execute(
                "state-add machine BurnLevel " +
                burnLevel.ToString(CultureInfo.InvariantCulture)).Succeeded);

            var assignment = harness.Processor.Execute(
                "player.Health = player.Health - " +
                "(machine.Heat * machine.BurnLevel)");

            AssertSuccessfulEvaluation(assignment, expectedHealth);
            Assert.Equal(expectedHealth, harness.Player.Health, 10);
            Assert.Same(harness.Player, harness.PlayerBinding.Value);
            Assert.Same(harness.Machine, harness.MachineBinding.Value);

            TestLog.State("BurnLevel: " + burnLevel);
            TestLog.State("Player.Health: 50 -> " + harness.Player.Health);
            TestLog.Pass("Native and attached state composed correctly.");
        }

        [Fact]
        public void Removing_one_runtime_property_makes_it_unresolved_again()
        {
            TestLog.Section("QSL COMMAND SURFACE - SELECTIVE REMOVAL");
            TestLog.Test("Removing BurnLevel preserves Heat and blocks dependent mutation.");

            var harness = CreateHarness();
            Assert.True(harness.Processor.Execute("state-add machine Heat 20").Succeeded);
            Assert.True(harness.Processor.Execute("state-add machine BurnLevel 1").Succeeded);

            var first = harness.Processor.Execute(
                "player.Health = player.Health - " +
                "(machine.Heat * machine.BurnLevel)");

            AssertSuccessfulEvaluation(first, 30.0);
            Assert.Equal(30.0, harness.Player.Health, 10);

            var removed = harness.Processor.Execute(
                "state-remove machine BurnLevel");

            Assert.Equal(QslCommandKind.StateRemoved, removed.Kind);
            Assert.True(removed.Succeeded);
            AssertFailedEvaluation(harness.Processor.Execute("machine.BurnLevel"));

            var failed = harness.Processor.Execute(
                "player.Health = player.Health - " +
                "(machine.Heat * machine.BurnLevel)");

            AssertFailedEvaluation(failed);
            Assert.Equal(30.0, harness.Player.Health, 10);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);

            TestLog.State("BurnLevel after removal: unresolved");
            TestLog.State("Heat after removal: 20");
            TestLog.State("Player.Health after failed retry: " + harness.Player.Health);
            TestLog.Pass("Selective removal preserved unrelated state and mutation honesty.");
        }

        [Fact]
        public void Removing_all_runtime_state_preserves_native_bound_properties()
        {
            TestLog.Section("QSL COMMAND SURFACE - REMOVE ALL STATE");
            TestLog.Test("Removing all attached state preserves native vocabulary.");

            var harness = CreateHarness();
            Assert.True(harness.Processor.Execute("state-add machine Heat 20").Succeeded);
            Assert.True(harness.Processor.Execute("state-add machine Radiation 15").Succeeded);
            Assert.True(harness.Processor.Execute("state-add machine BurnLevel 2").Succeeded);

            var removed = harness.Processor.Execute("state-remove machine");
            Assert.Equal(QslCommandKind.StateRemovedAll, removed.Kind);
            Assert.True(removed.Succeeded);

            AssertFailedEvaluation(harness.Processor.Execute("machine.Heat"));
            AssertFailedEvaluation(harness.Processor.Execute("machine.Radiation"));
            AssertFailedEvaluation(harness.Processor.Execute("machine.BurnLevel"));
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);
            AssertSuccessfulEvaluation(harness.Processor.Execute("action.Amount"), 10.0);

            TestLog.State("Machine attached state: removed");
            TestLog.State("Player.Health: " + harness.Player.Health);
            TestLog.State("Action.Amount: " + harness.Action.Amount);
            TestLog.Pass("Attached-state clear preserved native consumer state.");
        }

        [Fact]
        public void Gold_standard_native_and_attached_vocabulary_resolves_to_41_3()
        {
            TestLog.Section("QSL COMMAND SURFACE - GOLD STANDARD");
            TestLog.Test("Space Dust shorthand lifecycle resolves 50 to 41.3 honestly.");

            var harness = CreateHarness();
            const string goldenSource =
                "player.Health = player.Health - " +
                "(action.Amount * " +
                "(action.SpaceDustAppetite / 100))";

            TestLog.State("Player.Health starting: " + harness.Player.Health);
            TestLog.State("Action.Amount native: " + harness.Action.Amount);
            TestLog.State("SpaceDustAppetite before receipt: unresolved");

            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);
            AssertSuccessfulEvaluation(harness.Processor.Execute("action.Amount"), 10.0);
            AssertFailedEvaluation(harness.Processor.Execute("action.SpaceDustAppetite"));

            var premature = harness.Processor.Execute(goldenSource);
            AssertFailedEvaluation(premature);
            Assert.Equal(50.0, harness.Player.Health, 10);
            TestLog.State("Player.Health after premature failure: " + harness.Player.Health);

            var added = harness.Processor.Execute(
                "state-add action SpaceDustAppetite 87");

            Assert.Equal(QslCommandKind.StateAdded, added.Kind);
            Assert.True(added.Succeeded);
            Assert.Equal(87.0, Assert.IsType<double>(added.Value), 10);
            AssertSuccessfulEvaluation(
                harness.Processor.Execute("action.SpaceDustAppetite"),
                87.0);

            TestLog.State("SpaceDustAppetite after receipt: 87");
            TestLog.State("Golden source: " + goldenSource);

            var golden = harness.Processor.Execute(goldenSource);
            AssertSuccessfulEvaluation(golden, 41.3);
            Assert.Equal(41.3, harness.Player.Health, 10);
            Assert.Same(harness.Player, harness.PlayerBinding.Value);
            Assert.Same(harness.Action, harness.ActionBinding.Value);

            TestLog.State("Golden result: " + Format(golden.Value));
            TestLog.State("Player.Health after success: " + harness.Player.Health);

            var removed = harness.Processor.Execute(
                "state-remove action SpaceDustAppetite");

            Assert.Equal(QslCommandKind.StateRemoved, removed.Kind);
            Assert.True(removed.Succeeded);
            AssertFailedEvaluation(harness.Processor.Execute("action.SpaceDustAppetite"));

            var retired = harness.Processor.Execute(goldenSource);
            AssertFailedEvaluation(retired);
            Assert.Equal(41.3, harness.Player.Health, 10);
            AssertSuccessfulEvaluation(harness.Processor.Execute("action.Amount"), 10.0);
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 41.3);

            TestLog.State("SpaceDustAppetite after removal: unresolved");
            TestLog.State("Player.Health after failed retry: " + harness.Player.Health);
            TestLog.Pass("Golden shorthand lifecycle preserved exact state and failure honesty.");
        }

        [Fact]
        public void State_add_preserves_existing_value_parsing_semantics()
        {
            TestLog.Section("QSL COMMAND SURFACE - VALUE PARSING");
            TestLog.Test("Shorthand preserves number, boolean, null, quoted, and raw strings.");

            var harness = CreateHarness();
            var number = harness.Processor.Execute("state-add machine Number 87.5");
            var enabled = harness.Processor.Execute("state-add machine Enabled true");
            var disabled = harness.Processor.Execute("state-add machine Disabled false");
            var nothing = harness.Processor.Execute("state-add machine Nothing null");
            var label = harness.Processor.Execute("state-add machine Label \"Space Dust\"");
            var category = harness.Processor.Execute("state-add machine Category 'Hazard'");
            var raw = harness.Processor.Execute("state-add machine RawValue radiation");

            Assert.True(number.Succeeded);
            Assert.True(enabled.Succeeded);
            Assert.True(disabled.Succeeded);
            Assert.True(nothing.Succeeded);
            Assert.True(label.Succeeded);
            Assert.True(category.Succeeded);
            Assert.True(raw.Succeeded);

            Assert.Equal(87.5, Assert.IsType<double>(number.Value), 10);
            Assert.True(Assert.IsType<bool>(enabled.Value));
            Assert.False(Assert.IsType<bool>(disabled.Value));
            Assert.Null(nothing.Value);
            Assert.Equal("Space Dust", Assert.IsType<string>(label.Value));
            Assert.Equal("Hazard", Assert.IsType<string>(category.Value));
            Assert.Equal("radiation", Assert.IsType<string>(raw.Value));

            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Number"), 87.5);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Enabled"), true);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Disabled"), false);

            var nullResult = harness.Processor.Execute("machine.Nothing");
            Assert.Equal(QslCommandKind.Evaluation, nullResult.Kind);
            Assert.True(nullResult.Succeeded);
            Assert.NotNull(nullResult.ProcessingResult);
            Assert.True(nullResult.ProcessingResult!.Succeeded);
            Assert.Null(nullResult.Value);

            AssertSuccessfulEvaluation(
                harness.Processor.Execute("machine.Label"),
                "Space Dust");
            AssertSuccessfulEvaluation(
                harness.Processor.Execute("machine.Category"),
                "Hazard");
            AssertSuccessfulEvaluation(
                harness.Processor.Execute("machine.RawValue"),
                "radiation");

            TestLog.State("Number: " + Format(number.Value));
            TestLog.State("Enabled: " + Format(enabled.Value));
            TestLog.State("Disabled: " + Format(disabled.Value));
            TestLog.State("Nothing: null");
            TestLog.State("Label: " + Format(label.Value));
            TestLog.State("Category: " + Format(category.Value));
            TestLog.State("RawValue: " + Format(raw.Value));
            TestLog.Pass("Shorthand value parsing semantics were preserved.");
        }

        [Fact]
        public void Duplicate_state_add_is_non_destructive()
        {
            TestLog.Section("QSL COMMAND SURFACE - DUPLICATE RECEIPT");
            TestLog.Test("Duplicate receipt preserves the existing value.");

            var harness = CreateHarness();
            var first = harness.Processor.Execute("state-add machine Heat 20");
            var duplicate = harness.Processor.Execute("state-add machine Heat 99");

            Assert.True(first.Succeeded);
            Assert.False(duplicate.Succeeded);
            Assert.Equal(QslCommandKind.StateAdded, duplicate.Kind);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);

            TestLog.State("Initial Heat: 20");
            TestLog.State("Duplicate requested Heat: 99");
            TestLog.State("Authoritative Heat after duplicate: 20");
            TestLog.Pass("Duplicate receipt was non-destructive.");
        }

        [Fact]
        public void Unknown_target_is_rejected_without_creating_vocabulary()
        {
            TestLog.Section("QSL COMMAND SURFACE - UNKNOWN TARGET");
            TestLog.Test("Unknown targets are rejected without side effects.");

            var harness = CreateHarness();
            var added = harness.Processor.Execute("state-add missing Heat 20");
            var removed = harness.Processor.Execute("state-remove missing Heat");

            Assert.Equal(QslCommandKind.Error, added.Kind);
            Assert.False(added.Succeeded);
            Assert.Contains("could not be resolved", added.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(QslCommandKind.Error, removed.Kind);
            Assert.False(removed.Succeeded);
            AssertFailedEvaluation(harness.Processor.Execute("missing.Heat"));
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);

            TestLog.State("Add result: " + added.Message);
            TestLog.State("Remove result: " + removed.Message);
            TestLog.State("Player.Health remained: " + harness.Player.Health);
            TestLog.Pass("Unknown targets created no vocabulary or native mutation.");
        }

        [Theory]
        [InlineData("state-add", "Usage: state-add")]
        [InlineData("state-add machine", "Usage: state-add")]
        [InlineData("state-add machine Heat", "Usage: state-add")]
        [InlineData("state-remove", "Usage: state-remove")]
        [InlineData("state-remove machine Heat extra", "Usage: state-remove")]
        public void Malformed_shorthand_is_recognized_and_rejected_without_mutation(
            string source,
            string expectedMessage)
        {
            TestLog.Section("QSL COMMAND SURFACE - MALFORMED SHORTHAND");
            TestLog.Test("Known malformed shorthand is rejected before QSL evaluation.");
            TestLog.State("Source: " + source);

            var harness = CreateHarness();
            var handled = harness.Processor.TryExecuteCommand(source, out var result);
            LogCommandResult(handled, result);

            Assert.True(handled);
            Assert.NotNull(result);
            Assert.Equal(QslCommandKind.Error, result!.Kind);
            Assert.False(result.Succeeded);
            Assert.Contains(expectedMessage, result.Message, StringComparison.OrdinalIgnoreCase);
            AssertFailedEvaluation(harness.Processor.Execute("machine.Heat"));
            Assert.Equal(50.0, harness.Player.Health, 10);

            TestLog.Pass("Malformed shorthand was rejected without mutation.");
        }

        [Theory]
        [InlineData("help", QslCommandKind.Help)]
        [InlineData("HELP", QslCommandKind.Help)]
        [InlineData("clear", QslCommandKind.ClearDisplay)]
        [InlineData("CLEAR", QslCommandKind.ClearDisplay)]
        [InlineData("state", QslCommandKind.State)]
        [InlineData("STATE", QslCommandKind.State)]
        [InlineData("provenance", QslCommandKind.Provenance)]
        [InlineData("PROVENANCE", QslCommandKind.Provenance)]
        [InlineData("certify", QslCommandKind.Certify)]
        [InlineData("CERTIFY", QslCommandKind.Certify)]
        [InlineData("exit", QslCommandKind.Exit)]
        [InlineData("EXIT", QslCommandKind.Exit)]
        [InlineData("quit", QslCommandKind.Exit)]
        [InlineData("QUIT", QslCommandKind.Exit)]
        public void Existing_host_commands_are_recognized_case_insensitively(
            string source,
            QslCommandKind expectedKind)
        {
            TestLog.Section("QSL COMMAND SURFACE - HOST COMMAND");
            TestLog.Test("Host command recognition is case-insensitive.");
            TestLog.State("Source: " + source);

            var harness = CreateHarness();
            var result = harness.Processor.Execute(source);

            Assert.Equal(expectedKind, result.Kind);
            Assert.True(result.Succeeded);
            Assert.Equal(50.0, harness.Player.Health, 10);
            Assert.Equal(10.0, harness.Action.Amount, 10);

            TestLog.State("Kind: " + result.Kind);
            TestLog.Pass("Host command was recognized without consumer-state mutation.");
        }

        [Fact]
        public void State_command_returns_current_context_variables()
        {
            TestLog.Section("QSL COMMAND SURFACE - STATE INSPECTION");
            TestLog.Test("state returns the current context variable view.");

            var harness = CreateHarness();
            var result = harness.Processor.Execute("state");

            Assert.Equal(QslCommandKind.State, result.Kind);
            Assert.True(result.Succeeded);
            Assert.Same(harness.Context.Variables, result.Value);

            TestLog.State("Variable count: " + harness.Context.Variables.Count);
            TestLog.Pass("State inspection returned the current context view.");
        }

        [Fact]
        public void Clear_command_requests_display_clear_without_clearing_runtime()
        {
            TestLog.Section("QSL COMMAND SURFACE - CLEAR DISPLAY");
            TestLog.Test("clear requests display clearing without clearing runtime state.");

            var harness = CreateHarness();
            Assert.True(harness.Processor.Execute("state-add machine Heat 20").Succeeded);

            var clear = harness.Processor.Execute("clear");
            Assert.Equal(QslCommandKind.ClearDisplay, clear.Kind);
            Assert.True(clear.Succeeded);
            AssertSuccessfulEvaluation(harness.Processor.Execute("machine.Heat"), 20.0);
            AssertSuccessfulEvaluation(harness.Processor.Execute("player.Health"), 50.0);

            TestLog.State("machine.Heat after clear: 20");
            TestLog.State("Player.Health after clear: " + harness.Player.Health);
            TestLog.Pass("Display clear did not clear QSL runtime state.");
        }

        [Fact]
        public void Empty_source_is_rejected_without_evaluation()
        {
            TestLog.Section("QSL COMMAND SURFACE - EMPTY INPUT");
            TestLog.Test("Empty source is rejected without evaluation.");

            var harness = CreateHarness();
            var result = harness.Processor.Execute(" ");

            Assert.Equal(QslCommandKind.Error, result.Kind);
            Assert.False(result.Succeeded);
            Assert.Null(result.ProcessingResult);
            Assert.Equal(50.0, harness.Player.Health, 10);

            TestLog.State("Message: " + result.Message);
            TestLog.State("Player.Health remained: " + harness.Player.Health);
            TestLog.Pass("Empty source was rejected without evaluation.");
        }

        private static Harness CreateHarness()
        {
            var registry = new QslRuntimeRegistry();
            registry.RegisterDefaultEvaluators();

            var context = new QslRuntimeContext(registry);
            var engine = new QslEngine(context);
            var player = new TestPlayer(50.0, 25.0);
            var action = new TestAction(10.0);
            var machine = new TestMachine();

            var playerBinding = new QslConsumerBinding("player", "Player", player);
            var actionBinding = new QslConsumerBinding("action", "Action", action);
            var machineBinding = new QslConsumerBinding("machine", "Machine", machine);

            engine.BindConsumer(playerBinding);
            engine.BindConsumer(actionBinding);
            engine.BindConsumer(machineBinding);

            return new Harness(
                registry,
                context,
                engine,
                new QslCommandProcessor(engine, context),
                player,
                action,
                machine,
                playerBinding,
                actionBinding,
                machineBinding);
        }

        private static void AssertSuccessfulEvaluation(
            QslCommandResult result,
            double expected)
        {
            Assert.Equal(QslCommandKind.Evaluation, result.Kind);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.ProcessingResult);
            Assert.True(result.ProcessingResult!.Succeeded);
            Assert.Equal(expected, Convert.ToDouble(result.Value), 10);
        }

        private static void AssertSuccessfulEvaluation(
            QslCommandResult result,
            bool expected)
        {
            Assert.Equal(QslCommandKind.Evaluation, result.Kind);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.ProcessingResult);
            Assert.True(result.ProcessingResult!.Succeeded);
            Assert.Equal(expected, Assert.IsType<bool>(result.Value));
        }

        private static void AssertSuccessfulEvaluation(
            QslCommandResult result,
            string expected)
        {
            Assert.Equal(QslCommandKind.Evaluation, result.Kind);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.ProcessingResult);
            Assert.True(result.ProcessingResult!.Succeeded);
            Assert.Equal(expected, Assert.IsType<string>(result.Value));
        }

        private static void AssertFailedEvaluation(QslCommandResult result)
        {
            Assert.Equal(QslCommandKind.Evaluation, result.Kind);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.ProcessingResult);
            Assert.False(result.ProcessingResult!.Succeeded);
        }

        private static void LogCommandResult(
            bool handled,
            QslCommandResult? result)
        {
            TestLog.State("Handled as command: " + handled);
            TestLog.State("Result present: " + (result is not null));

            if (result is null)
                return;

            TestLog.State("Kind: " + result.Kind);
            TestLog.State("Succeeded: " + result.Succeeded);
            TestLog.State("Message: " + result.Message);
            TestLog.State("Value: " + Format(result.Value));
        }

        private static void LogEvaluationFailure(
            string label,
            QslCommandResult result)
        {
            TestLog.State(label + " succeeded: " + result.Succeeded);

            if (result.ProcessingResult is null)
                return;

            TestLog.State(
                label + " runtime diagnostics: " +
                result.ProcessingResult.RuntimeDiagnostics.Count);
        }

        private static string Format(object? value)
        {
            return value is null
                ? "<null>"
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<null>";
        }

        private sealed class Harness
        {
            public Harness(
                QslRuntimeRegistry registry,
                QslRuntimeContext context,
                QslEngine engine,
                QslCommandProcessor processor,
                TestPlayer player,
                TestAction action,
                TestMachine machine,
                QslConsumerBinding playerBinding,
                QslConsumerBinding actionBinding,
                QslConsumerBinding machineBinding)
            {
                Registry = registry;
                Context = context;
                Engine = engine;
                Processor = processor;
                Player = player;
                Action = action;
                Machine = machine;
                PlayerBinding = playerBinding;
                ActionBinding = actionBinding;
                MachineBinding = machineBinding;
            }

            public QslRuntimeRegistry Registry { get; }
            public QslRuntimeContext Context { get; }
            public QslEngine Engine { get; }
            public QslCommandProcessor Processor { get; }
            public TestPlayer Player { get; }
            public TestAction Action { get; }
            public TestMachine Machine { get; }
            public QslConsumerBinding PlayerBinding { get; }
            public QslConsumerBinding ActionBinding { get; }
            public QslConsumerBinding MachineBinding { get; }
        }

        private sealed class TestPlayer
        {
            public TestPlayer(double health, double mana)
            {
                Health = health;
                Mana = mana;
            }

            public double Health { get; set; }
            public double Mana { get; set; }
        }

        private sealed class TestAction
        {
            public TestAction(double amount)
            {
                Amount = amount;
            }

            public double Amount { get; }
        }

        private sealed class TestMachine
        {
        }
    }
}
