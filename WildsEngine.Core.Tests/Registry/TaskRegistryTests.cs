using System;
using System.Diagnostics;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Processing.Registry;
using WildsEngine.Core.Tests;
using WildsEngine.Tests.Logging;

namespace WildsEngine.Tests.Registry
{
    /// <summary>
    /// Certifies task registry storage, retrieval, enumeration,
    /// duplicate protection, removal, and high-volume behavior.
    /// </summary>
    public static class TaskRegistryTests
    {
        private static readonly int[] Volumes =
        {
            1_000,
            10_000,
            100_000,
            1_000_000
        };

        public static void RunAll()
        {
            TestLog.Write(
                "=== Task Registry Tests ===");

            TestLog.Write(
                string.Empty);

            BasicTaskLifecycle();

            for (var i = 0; i < Volumes.Length; i++)
            {
                HighVolumeTaskLifecycle(
                    Volumes[i]);
            }

            TestLog.Write(
                string.Empty);

            TestLog.Write(
                $"=== Task Registry Tests Passed: " +
                $"{Volumes.Length + 1}/{Volumes.Length + 1} ===");
        }

        private static void BasicTaskLifecycle()
        {
            const string testName =
                "Task registry basic lifecycle";

            TestLog.Write(
                $"[TEST] {testName}");

            var component =
                CreateComponent(
                    "test.task-registry.basic");

            var registry =
                new DefaultTasksRegistry(
                    component,
                    "test.task-registry.basic");

            TestLog.Write(
                "    [TASK] Initializing task registry");

            Assert(
                registry.Initialize(),
                "First registry initialization should succeed.");

            Assert(
                registry.IsInitialized,
                "Registry should report initialized.");

            Assert(
                registry.Count == 0,
                "Registry should begin empty.");

            var task =
                new TestTask(
                    component,
                    "test.task.basic");

            TestLog.Write(
                $"    [PARK] Task {task.Id}");

            Assert(
                registry.Register(task),
                "Task should register successfully.");

            Assert(
                registry.Count == 1,
                "Registry count should become one.");

            Assert(
                registry.Contains(task.Id),
                "Registry should contain the parked task.");

            var retrieved =
                registry.Get(
                    task.Id);

            TestLog.Write(
                $"    [READ] Task {task.Id}");

            Assert(
                retrieved != null,
                "Parked task should be retrievable.");

            if (retrieved == null)
            {
                throw new InvalidOperationException(
                    "TaskRegistry test failed: " +
                    "Parked task retrieval returned null.");
            }

            Assert(
                ReferenceEquals(
                    task,
                    retrieved),
                "Retrieved task should be the parked instance.");

            Assert(
                registry.TryGet(
                    task.Id,
                    out var tried),
                "TryGet should find the parked task.");

            Assert(
                tried != null,
                "TryGet should return a task.");

            Assert(
                ReferenceEquals(
                    task,
                    tried),
                "TryGet should return the parked instance.");

            Assert(
                !registry.Register(task),
                "Duplicate task registration should be rejected.");

            Assert(
                registry.Count == 1,
                "Duplicate registration must not change count.");

            TestLog.Write(
                $"    [REMOVE] Task {task.Id}");

            Assert(
                registry.Remove(task.Id),
                "Parked task should be removable.");

            Assert(
                registry.Count == 0,
                "Registry should be empty after removal.");

            Assert(
                !registry.Contains(task.Id),
                "Removed task should no longer be contained.");

            Assert(
                registry.Get(task.Id) == null,
                "Removed task should no longer be retrievable.");

            Assert(
                !registry.Remove(task.Id),
                "Removing an absent task should return false.");

            Assert(
                registry.Shutdown(),
                "First registry shutdown should succeed.");

            Assert(
                !registry.IsInitialized,
                "Registry should report stopped.");

            TestLog.Write(
                $"[PASS] {testName}");
        }

        private static void HighVolumeTaskLifecycle(
            int taskCount)
        {
            var testName =
                $"Task registry lifecycle at {taskCount:N0}";

            TestLog.Write(
                $"[TEST] {testName}");

            var component =
                CreateComponent(
                    $"test.task-registry.{taskCount}");

            var registry =
                new DefaultTasksRegistry(
                    component,
                    $"test.task-registry.{taskCount}");

            Assert(
                registry.Initialize(),
                "Registry initialization should succeed.");

            var taskIds =
                new Guid[taskCount];

            var registrationWatch =
                Stopwatch.StartNew();

            TestLog.Write(
                $"    [TASK] Parking {taskCount:N0} tasks");

            for (var i = 0; i < taskCount; i++)
            {
                var task =
                    new TestTask(
                        component,
                        "test.task.parked");

                taskIds[i] =
                    task.Id;

                if (!registry.Register(task))
                {
                    throw new InvalidOperationException(
                        $"Task registration failed at index {i:N0}.");
                }
            }

            registrationWatch.Stop();

            TestLog.Write(
                $"    [STATE] Parked count = {registry.Count:N0}");

            TestLog.Write(
                $"    [TIME] Registration = " +
                $"{registrationWatch.Elapsed.TotalMilliseconds:F3} ms");

            Assert(
                registry.Count == taskCount,
                $"Registry should contain {taskCount:N0} tasks.");

            Assert(
                registry.Items.Count == taskCount,
                "Items should expose every parked task.");

            VerifyRepresentativeTasks(
                registry,
                taskIds);

            var duplicateId =
                taskIds[taskCount / 2];

            var duplicateTask =
                registry.Get(
                    duplicateId);

            Assert(
                duplicateTask != null,
                "Representative task should be available.");

            if (duplicateTask == null)
            {
                throw new InvalidOperationException(
                    "TaskRegistry test failed: " +
                    "Representative task retrieval returned null.");
            }

            Assert(
                !registry.Register(duplicateTask),
                "Duplicate task registration should be rejected.");

            Assert(
                registry.Count == taskCount,
                "Duplicate registration must not alter count.");

            var retrievalWatch =
                Stopwatch.StartNew();

            TestLog.Write(
                $"    [TASK] Reading {taskCount:N0} parked tasks");

            for (var i = 0; i < taskIds.Length; i++)
            {
                if (!registry.TryGet(
                        taskIds[i],
                        out var task))
                {
                    throw new InvalidOperationException(
                        $"Task retrieval failed at index {i:N0}.");
                }

                if (task == null)
                {
                    throw new InvalidOperationException(
                        $"Task retrieval returned null at index {i:N0}.");
                }

                if (task.Id != taskIds[i])
                {
                    throw new InvalidOperationException(
                        $"Task identity mismatch at index {i:N0}.");
                }
            }

            retrievalWatch.Stop();

            TestLog.Write(
                $"    [TIME] Retrieval = " +
                $"{retrievalWatch.Elapsed.TotalMilliseconds:F3} ms");

            var removalWatch =
                Stopwatch.StartNew();

            TestLog.Write(
                $"    [TASK] Removing {taskCount:N0} parked tasks");

            for (var i = 0; i < taskIds.Length; i++)
            {
                if (!registry.Remove(
                        taskIds[i]))
                {
                    throw new InvalidOperationException(
                        $"Task removal failed at index {i:N0}.");
                }
            }

            removalWatch.Stop();

            TestLog.Write(
                $"    [STATE] Remaining count = {registry.Count:N0}");

            TestLog.Write(
                $"    [TIME] Removal = " +
                $"{removalWatch.Elapsed.TotalMilliseconds:F3} ms");

            Assert(
                registry.Count == 0,
                "Registry should be empty after every task is removed.");

            Assert(
                registry.Items.Count == 0,
                "Items should be empty after removal.");

            VerifyTasksAreAbsent(
                registry,
                taskIds);

            Assert(
                registry.Shutdown(),
                "Registry shutdown should succeed.");

            TestLog.Write(
                $"[PASS] {testName}");
        }

        private static void VerifyRepresentativeTasks(
            ITaskRegistry registry,
            Guid[] taskIds)
        {
            var indexes =
                new[]
                {
                    0,
                    taskIds.Length / 2,
                    taskIds.Length - 1
                };

            for (var i = 0; i < indexes.Length; i++)
            {
                var index =
                    indexes[i];

                var id =
                    taskIds[index];

                TestLog.Write(
                    $"    [READ] Representative task " +
                    $"{index:N0}: {id}");

                Assert(
                    registry.Contains(id),
                    $"Registry should contain task at index {index:N0}.");

                var task =
                    registry.Get(id);

                Assert(
                    task != null,
                    $"Task at index {index:N0} should be retrievable.");

                if (task == null)
                {
                    throw new InvalidOperationException(
                        "TaskRegistry test failed: " +
                        $"Task at index {index:N0} returned null.");
                }

                Assert(
                    task.Id == id,
                    $"Task at index {index:N0} should preserve identity.");
            }
        }

        private static void VerifyTasksAreAbsent(
            ITaskRegistry registry,
            Guid[] taskIds)
        {
            var indexes =
                new[]
                {
                    0,
                    taskIds.Length / 2,
                    taskIds.Length - 1
                };

            for (var i = 0; i < indexes.Length; i++)
            {
                var index =
                    indexes[i];

                var id =
                    taskIds[index];

                TestLog.Write(
                    $"    [VERIFY] Removed task " +
                    $"{index:N0}: {id}");

                Assert(
                    !registry.Contains(id),
                    $"Removed task at index {index:N0} " +
                    "should not be contained.");

                Assert(
                    registry.Get(id) == null,
                    $"Removed task at index {index:N0} " +
                    "should not be retrievable.");

                Assert(
                    !registry.TryGet(
                        id,
                        out var task),
                    $"TryGet should reject removed task " +
                    $"at index {index:N0}.");

                Assert(
                    task == null,
                    "Failed TryGet should return null.");
            }
        }

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    $"TaskRegistry test failed: {message}");
            }
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