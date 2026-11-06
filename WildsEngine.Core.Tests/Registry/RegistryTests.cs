using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Tests;
using WildsEngine.Tests.Logging;

namespace WildsEngine.Tests.Registry
{
    /// <summary>
    /// Dependency-free certification suite for registry storage.
    ///
    /// Certifies that instructions and tasks can be parked,
    /// inspected, retrieved, removed, cleared, and manipulated
    /// concurrently without losing registry consistency.
    /// </summary>
    public static class RegistryTests
    {
        private const int ConcurrentItemCount = 1000;

        public static async Task RunAllAsync()
        {
            TestLog.Write(
                "=== Registry Tests ===");

            TestLog.Write(
                string.Empty);

            RegistryStartsEmpty();
            ParkInstruction();
            CountBecomesOne();
            EnumerateParkedInstruction();
            RetrieveInstructionByGuid();
            VerifyInstructionReferenceIdentity();
            RejectDuplicateRegistration();
            ParkMultipleInstructions();
            ObserveAllParkedInstructions();
            RemoveInstruction();
            VerifyRemovedInstructionIsGone();
            VerifyRemainingInstructionsAreUnaffected();
            ClearInstructionRegistry();
            VerifyInstructionRegistryIsEmpty();
            TaskRegistrySupportsCoreBehavior();

            await ConcurrentRegistrationAndRemovalStressAsync()
                .ConfigureAwait(false);

            TestLog.Write(
                string.Empty);

            TestLog.Write(
                "=== Registry Tests Passed: 16/16 ===");
        }

        private static void RegistryStartsEmpty()
        {
            TestLog.Write(
                "[TEST] Registry starts empty");

            var component =
                CreateComponent(
                    "certification.registry.empty");

            var registry =
                CreateInstructionRegistry(
                    component);

            PrintInstructionRegistry(
                registry,
                "Initial registry state");

            Assert(
                registry.Count == 0,
                "A new registry should have a count of zero.");

            Assert(
                registry.Items.Count == 0,
                "A new registry should expose no parked items.");

            TestLog.Pass(
                "Registry starts empty");
        }

        private static void ParkInstruction()
        {
            TestLog.Write(
                "[TEST] Park instruction");

            var component =
                CreateComponent(
                    "certification.registry.park");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.park");

            TestLog.Write(
                $"    [PARK] {DescribeInstruction(instruction)}");

            var registered =
                registry.Register(
                    instruction);

            Assert(
                registered,
                "The first registration should succeed.");

            Assert(
                registry.Contains(
                    instruction.Id),
                "The registry should contain the parked instruction.");

            PrintInstructionRegistry(
                registry,
                "After instruction park");

            TestLog.Pass(
                "Park instruction");
        }

        private static void CountBecomesOne()
        {
            TestLog.Write(
                "[TEST] Count becomes 1");

            var component =
                CreateComponent(
                    "certification.registry.count");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.count");

            registry.Register(
                instruction);

            TestLog.Write(
                $"    [STATE] Count = {registry.Count}");

            Assert(
                registry.Count == 1,
                "Parking one instruction should produce a count of one.");

            TestLog.Pass(
                "Count becomes 1");
        }

        private static void EnumerateParkedInstruction()
        {
            TestLog.Write(
                "[TEST] Enumerate Items and print parked instruction");

            var component =
                CreateComponent(
                    "certification.registry.enumerate");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.enumerate");

            registry.Register(
                instruction);

            var observedCount = 0;

            foreach (var parked in registry.Items)
            {
                observedCount++;

                TestLog.Write(
                    $"    [ITEM] {DescribeInstruction(parked)}");
            }

            Assert(
                observedCount == 1,
                "Items should enumerate the parked instruction.");

            TestLog.Pass(
                "Enumerate Items and print parked instruction");
        }

        private static void RetrieveInstructionByGuid()
        {
            TestLog.Write(
                "[TEST] Retrieve instruction by GUID");

            var component =
                CreateComponent(
                    "certification.registry.retrieve");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.retrieve");

            registry.Register(
                instruction);

            TestLog.Write(
                $"    [READ] Instruction {instruction.Id}");

            var retrieved =
                registry.Get(
                    instruction.Id);

            Assert(
                retrieved != null,
                "Get should retrieve a parked instruction.");

            if (retrieved == null)
            {
                throw new InvalidOperationException(
                    "Registry test failed: " +
                    "Get returned null for a parked instruction.");
            }

            Assert(
                retrieved.Id == instruction.Id,
                "The retrieved instruction should have the requested GUID.");

            var found =
                registry.TryGet(
                    instruction.Id,
                    out var tried);

            Assert(
                found,
                "TryGet should report the parked instruction.");

            Assert(
                tried != null,
                "TryGet should supply the parked instruction.");

            TestLog.Write(
                $"    [FOUND] {DescribeInstruction(retrieved)}");

            TestLog.Pass(
                "Retrieve instruction by GUID");
        }

        private static void VerifyInstructionReferenceIdentity()
        {
            TestLog.Write(
                "[TEST] Verify instruction reference and identity");

            var component =
                CreateComponent(
                    "certification.registry.identity");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.identity");

            registry.Register(
                instruction);

            var retrieved =
                registry.Get(
                    instruction.Id);

            var sameReference =
                ReferenceEquals(
                    instruction,
                    retrieved);

            TestLog.Write(
                $"    [STATE] Same reference = {sameReference}");

            Assert(
                sameReference,
                "The registry should preserve the parked object reference.");

            Assert(
                retrieved != null &&
                retrieved.Id == instruction.Id,
                "The parked instruction identity should remain unchanged.");

            TestLog.Pass(
                "Verify instruction reference and identity");
        }

        private static void RejectDuplicateRegistration()
        {
            TestLog.Write(
                "[TEST] Reject duplicate registration");

            var component =
                CreateComponent(
                    "certification.registry.duplicate");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.duplicate");

            var first =
                registry.Register(
                    instruction);

            var second =
                registry.Register(
                    instruction);

            TestLog.Write(
                $"    [STATE] First registration = {first}");

            TestLog.Write(
                $"    [STATE] Duplicate registration = {second}");

            Assert(
                first,
                "The first registration should succeed.");

            Assert(
                !second,
                "The duplicate GUID registration should be rejected.");

            Assert(
                registry.Count == 1,
                "Duplicate registration must not increase registry count.");

            Assert(
                ReferenceEquals(
                    instruction,
                    registry.Get(
                        instruction.Id)),
                "Duplicate rejection must not replace the original item.");

            TestLog.Pass(
                "Reject duplicate registration");
        }

        private static void ParkMultipleInstructions()
        {
            TestLog.Write(
                "[TEST] Park multiple instructions");

            var component =
                CreateComponent(
                    "certification.registry.multiple");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instructions =
                CreateInstructionSet(
                    4,
                    "certification.instruction.multiple");

            foreach (var instruction in instructions)
            {
                var registered =
                    registry.Register(
                        instruction);

                Assert(
                    registered,
                    $"Instruction {instruction.Id} should be parked.");

                TestLog.Write(
                    $"    [PARK] {DescribeInstruction(instruction)}");
            }

            Assert(
                registry.Count == instructions.Count,
                "Every unique instruction should be parked.");

            PrintInstructionRegistry(
                registry,
                "After multiple instruction parks");

            TestLog.Pass(
                "Park multiple instructions");
        }

        private static void ObserveAllParkedInstructions()
        {
            TestLog.Write(
                "[TEST] Observe all currently parked objects");

            var component =
                CreateComponent(
                    "certification.registry.observe");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instructions =
                CreateInstructionSet(
                    5,
                    "certification.instruction.observe");

            foreach (var instruction in instructions)
            {
                registry.Register(
                    instruction);
            }

            var parked =
                registry.Items.ToArray();

            TestLog.Write(
                $"    [STATE] Parked instruction count = " +
                $"{parked.Length}");

            foreach (var instruction in parked)
            {
                TestLog.Write(
                    $"    [ITEM] {DescribeInstruction(instruction)}");
            }

            Assert(
                parked.Length == instructions.Count,
                "Items should expose every parked instruction.");

            foreach (var expected in instructions)
            {
                Assert(
                    parked.Any(
                        candidate =>
                            candidate.Id == expected.Id),
                    $"Instruction {expected.Id} should be observable.");
            }

            TestLog.Pass(
                "Observe all currently parked objects");
        }

        private static void RemoveInstruction()
        {
            TestLog.Write(
                "[TEST] Remove instruction");

            var component =
                CreateComponent(
                    "certification.registry.remove");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.remove");

            registry.Register(
                instruction);

            TestLog.Write(
                $"    [REMOVE] Instruction {instruction.Id}");

            var removed =
                registry.Remove(
                    instruction.Id);

            Assert(
                removed,
                "Removing a parked instruction should succeed.");

            Assert(
                registry.Count == 0,
                "Removing the only instruction should empty the registry.");

            TestLog.Pass(
                "Remove instruction");
        }

        private static void VerifyRemovedInstructionIsGone()
        {
            TestLog.Write(
                "[TEST] Verify removed instruction is gone");

            var component =
                CreateComponent(
                    "certification.registry.gone");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.gone");

            registry.Register(
                instruction);

            registry.Remove(
                instruction.Id);

            Assert(
                !registry.Contains(
                    instruction.Id),
                "Contains should be false after removal.");

            Assert(
                registry.Get(
                    instruction.Id) == null,
                "Get should return null after removal.");

            var found =
                registry.TryGet(
                    instruction.Id,
                    out var retrieved);

            Assert(
                !found,
                "TryGet should return false after removal.");

            Assert(
                retrieved == null,
                "TryGet should not return an item after removal.");

            TestLog.Write(
                $"    [STATE] Contains = " +
                $"{registry.Contains(instruction.Id)}");

            TestLog.Write(
                $"    [STATE] Count = {registry.Count}");

            TestLog.Pass(
                "Verify removed instruction is gone");
        }

        private static void VerifyRemainingInstructionsAreUnaffected()
        {
            TestLog.Write(
                "[TEST] Verify remaining items unaffected");

            var component =
                CreateComponent(
                    "certification.registry.remaining");

            var registry =
                CreateInstructionRegistry(
                    component);

            var first =
                CreateInstruction(
                    "certification.instruction.remaining.first");

            var removed =
                CreateInstruction(
                    "certification.instruction.remaining.removed");

            var third =
                CreateInstruction(
                    "certification.instruction.remaining.third");

            registry.Register(
                first);

            registry.Register(
                removed);

            registry.Register(
                third);

            registry.Remove(
                removed.Id);

            Assert(
                registry.Count == 2,
                "Removing one instruction should leave two parked.");

            Assert(
                registry.Contains(
                    first.Id),
                "The first instruction should remain.");

            Assert(
                registry.Contains(
                    third.Id),
                "The third instruction should remain.");

            Assert(
                !registry.Contains(
                    removed.Id),
                "Only the selected instruction should be removed.");

            Assert(
                ReferenceEquals(
                    first,
                    registry.Get(first.Id)),
                "The first parked reference should remain unchanged.");

            Assert(
                ReferenceEquals(
                    third,
                    registry.Get(third.Id)),
                "The third parked reference should remain unchanged.");

            PrintInstructionRegistry(
                registry,
                "After selective removal");

            TestLog.Pass(
                "Verify remaining items unaffected");
        }

        private static void ClearInstructionRegistry()
        {
            TestLog.Write(
                "[TEST] Clear registry");

            var component =
                CreateComponent(
                    "certification.registry.clear");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instructions =
                CreateInstructionSet(
                    8,
                    "certification.instruction.clear");

            foreach (var instruction in instructions)
            {
                registry.Register(
                    instruction);
            }

            Assert(
                registry.Count == instructions.Count,
                "Instructions should be parked before Clear.");

            TestLog.Write(
                $"    [STATE] Count before Clear = " +
                $"{registry.Count}");

            registry.Clear();

            TestLog.Write(
                $"    [STATE] Count after Clear = " +
                $"{registry.Count}");

            Assert(
                registry.Count == 0,
                "Clear should remove every parked instruction.");

            TestLog.Pass(
                "Clear registry");
        }

        private static void VerifyInstructionRegistryIsEmpty()
        {
            TestLog.Write(
                "[TEST] Verify registry empty");

            var component =
                CreateComponent(
                    "certification.registry.final-empty");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instruction =
                CreateInstruction(
                    "certification.instruction.empty");

            registry.Register(
                instruction);

            registry.Clear();

            Assert(
                registry.Count == 0,
                "Cleared registry should report zero items.");

            Assert(
                registry.Items.Count == 0,
                "Cleared registry should enumerate no items.");

            Assert(
                !registry.Contains(
                    instruction.Id),
                "Cleared registry should not contain the prior instruction.");

            Assert(
                registry.Get(
                    instruction.Id) == null,
                "Cleared registry should not retrieve the prior instruction.");

            PrintInstructionRegistry(
                registry,
                "Final empty state");

            TestLog.Pass(
                "Verify registry empty");
        }

        private static void TaskRegistrySupportsCoreBehavior()
        {
            TestLog.Write(
                "[TEST] Repeat core behavior for task registry");

            var component =
                CreateComponent(
                    "certification.task");

            var registry =
                CreateTaskRegistry(
                    component);

            Assert(
                registry.Count == 0,
                "A new task registry should start empty.");

            var first =
                new TestTask(
                    component,
                    "certification.task.first");

            var second =
                new TestTask(
                    component,
                    "certification.task.second");

            TestLog.Write(
                $"    [PARK] {DescribeTask(first)}");

            TestLog.Write(
                $"    [PARK] {DescribeTask(second)}");

            Assert(
                registry.Register(
                    first),
                "The first task should be parked.");

            Assert(
                registry.Register(
                    second),
                "The second task should be parked.");

            Assert(
                registry.Count == 2,
                "The task registry should contain both tasks.");

            Assert(
                ReferenceEquals(
                    first,
                    registry.Get(first.Id)),
                "The task registry should preserve reference identity.");

            Assert(
                !registry.Register(
                    first),
                "Duplicate task registration should be rejected.");

            foreach (var task in registry.Items)
            {
                TestLog.Write(
                    $"    [ITEM] {DescribeTask(task)}");
            }

            TestLog.Write(
                $"    [REMOVE] Task {first.Id}");

            Assert(
                registry.Remove(
                    first.Id),
                "Removing the first task should succeed.");

            Assert(
                !registry.Contains(
                    first.Id),
                "The removed task should no longer be parked.");

            Assert(
                registry.Contains(
                    second.Id),
                "The remaining task should be unaffected.");

            registry.Clear();

            Assert(
                registry.Count == 0,
                "Clearing should empty the task registry.");

            Assert(
                registry.Items.Count == 0,
                "The task registry should enumerate no items after Clear.");

            TestLog.Pass(
                "Repeat core behavior for task registry");
        }

        private static async Task
            ConcurrentRegistrationAndRemovalStressAsync()
        {
            TestLog.Write(
                "[TEST] Concurrent registration/removal stress");

            var component =
                CreateComponent(
                    "certification.registry.concurrent");

            var registry =
                CreateInstructionRegistry(
                    component);

            var instructions =
                CreateInstructionSet(
                    ConcurrentItemCount,
                    "certification.instruction.concurrent");

            TestLog.Write(
                $"    [TASK] Concurrently parking " +
                $"{instructions.Count} instructions");

            var registrationTasks =
                new List<Task>(
                    instructions.Count);

            foreach (var instruction in instructions)
            {
                registrationTasks.Add(
                    Task.Run(
                        () =>
                        {
                            var registered =
                                registry.Register(
                                    instruction);

                            Assert(
                                registered,
                                $"Concurrent registration failed " +
                                $"for {instruction.Id}.");
                        }));
            }

            await Task.WhenAll(
                    registrationTasks)
                .ConfigureAwait(false);

            TestLog.Write(
                $"    [STATE] Count after registration = " +
                $"{registry.Count}");

            Assert(
                registry.Count == instructions.Count,
                "Concurrent registration should retain " +
                "every unique instruction.");

            foreach (var instruction in instructions)
            {
                Assert(
                    registry.Contains(
                        instruction.Id),
                    $"Instruction {instruction.Id} should be parked.");
            }

            var removedInstructions =
                instructions
                    .Where(
                        (_, index) =>
                            index % 2 == 0)
                    .ToArray();

            var retainedInstructions =
                instructions
                    .Where(
                        (_, index) =>
                            index % 2 != 0)
                    .ToArray();

            TestLog.Write(
                $"    [TASK] Concurrently removing " +
                $"{removedInstructions.Length} instructions");

            var removalTasks =
                new List<Task>(
                    removedInstructions.Length);

            foreach (var instruction in removedInstructions)
            {
                removalTasks.Add(
                    Task.Run(
                        () =>
                        {
                            var removed =
                                registry.Remove(
                                    instruction.Id);

                            Assert(
                                removed,
                                $"Concurrent removal failed " +
                                $"for {instruction.Id}.");
                        }));
            }

            await Task.WhenAll(
                    removalTasks)
                .ConfigureAwait(false);

            TestLog.Write(
                $"    [STATE] Count after removal = " +
                $"{registry.Count}");

            Assert(
                registry.Count == retainedInstructions.Length,
                "Concurrent removal should leave the " +
                "expected instruction count.");

            foreach (var instruction in removedInstructions)
            {
                Assert(
                    !registry.Contains(
                        instruction.Id),
                    $"Removed instruction {instruction.Id} " +
                    "should be absent.");
            }

            foreach (var instruction in retainedInstructions)
            {
                Assert(
                    registry.Contains(
                        instruction.Id),
                    $"Retained instruction {instruction.Id} " +
                    "should remain parked.");

                Assert(
                    ReferenceEquals(
                        instruction,
                        registry.Get(
                            instruction.Id)),
                    $"Retained instruction {instruction.Id} " +
                    "should preserve its reference.");
            }

            registry.Clear();

            Assert(
                registry.Count == 0,
                "The stressed registry should remain clearable.");

            TestLog.Pass(
                "Concurrent registration/removal stress");
        }

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static TestInstructionRegistry
            CreateInstructionRegistry(
                IComponent component)
        {
            return new TestInstructionRegistry(
                component,
                "certification.instruction-registry");
        }

        private static TestTaskRegistry
            CreateTaskRegistry(
                IComponent component)
        {
            return new TestTaskRegistry(
                component,
                "certification.task-registry");
        }

        private static TestInstruction
            CreateInstruction(
                string intent)
        {
            return new TestInstruction(
                intent,
                payload: null);
        }

        private static List<TestInstruction>
            CreateInstructionSet(
                int count,
                string intentPrefix)
        {
            var instructions =
                new List<TestInstruction>(
                    count);

            for (var i = 0; i < count; i++)
            {
                instructions.Add(
                    new TestInstruction(
                        $"{intentPrefix}.{i:D4}",
                        payload: i));
            }

            return instructions;
        }

        private static string DescribeInstruction(
            IStateInstruction instruction)
        {
            return
                $"Instruction " +
                $"Id={instruction.Id}, " +
                $"Intent={instruction.Intent}";
        }

        private static string DescribeTask(
            ITask task)
        {
            return
                $"Task " +
                $"Id={task.Id}, " +
                $"Slug={task.Slug}";
        }

        private static void PrintInstructionRegistry(
            TestInstructionRegistry registry,
            string label)
        {
            TestLog.Write(
                $"    [REGISTRY] {label}");

            TestLog.Write(
                $"               Count : {registry.Count}");

            foreach (var instruction in registry.Items)
            {
                TestLog.Write(
                    $"               Item  : " +
                    $"{DescribeInstruction(instruction)}");
            }
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    $"Registry test failed: {message}");
            }
        }

        private sealed class TestInstructionRegistry :
            RegistryBase<IStateInstruction>
        {
            public TestInstructionRegistry(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer,
                    instruction => instruction.Id)
            {
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnShutdown()
            {
                Clear();
            }
        }

        private sealed class TestTaskRegistry :
            RegistryBase<ITask>
        {
            public TestTaskRegistry(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer,
                    task => task.Id)
            {
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnShutdown()
            {
                Clear();
            }
        }

        private sealed class TestInstruction :
            IStateInstruction
        {
            private readonly IReadOnlyDictionary<string, object?>
                _arguments;

            private readonly IReadOnlyDictionary<string, object?>
                _metadata;

            public TestInstruction(
                string intent,
                object? payload)
            {
                if (string.IsNullOrWhiteSpace(intent))
                {
                    throw new ArgumentException(
                        "Test instruction requires an intent.",
                        nameof(intent));
                }

                Id = Guid.NewGuid();
                Intent = intent;
                Payload = payload;

                _arguments =
                    new Dictionary<string, object?>();

                _metadata =
                    new Dictionary<string, object?>();
            }

            public Guid Id { get; }

            public string Intent { get; }

            public object? Payload { get; }

            public IReadOnlyDictionary<string, object?>
                Arguments =>
                    _arguments;

            public IReadOnlyDictionary<string, object?>
                Metadata =>
                    _metadata;
        }

        private sealed class TestTask :
            TaskBase
        {
            public TestTask(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer)
            {
            }
        }
    }
}