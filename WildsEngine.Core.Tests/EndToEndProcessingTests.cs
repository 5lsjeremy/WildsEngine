using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies the current complete processing route:
    ///
    /// WildsEngine
    ///     -> instruction processor lease
    ///     -> instruction registry
    ///     -> task processor lease
    ///     -> task registry
    ///     -> processor return
    ///     -> instruction removal
    ///
    /// These tests intentionally use the current composition surface.
    /// They do not invent the future WildsEngine.Process(...) contract.
    /// </summary>
    public static class EndToEndProcessingTests
    {
        private static readonly List<string> Passed =
            new List<string>();

        public static async Task RunAllAsync()
        {
            Passed.Clear();

            TestLog.Write(
                "=== End-to-End Processing Tests ===");

            await RunAsync(
                    "Instruction flows through complete processing route",
                    InstructionFlowsThroughCompleteProcessingRoute)
                .ConfigureAwait(false);

            await RunAsync(
                    "Processing failure returns processors and removes instruction",
                    ProcessingFailureReturnsProcessorsAndRemovesInstruction)
                .ConfigureAwait(false);

            await RunAsync(
                    "Processing cancellation returns processors and removes instruction",
                    ProcessingCancellationReturnsProcessorsAndRemovesInstruction)
                .ConfigureAwait(false);

            TestLog.Write(
                $"End-to-end processing tests passed: " +
                $"{Passed.Count}/{Passed.Count}");
        }

        private static async Task
            InstructionFlowsThroughCompleteProcessingRoute()
        {
            var component =
                CreateComponent(
                    "certification.end-to-end.success");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var instruction =
                new SetContextVariableInstruction(
                    "PlayerHealth",
                    10);

            IInstructionsRegistry? observedInstructionRegistry =
                null;

            ITaskRegistry? observedTaskRegistry =
                null;

            ITaskProcessor? observedTaskProcessor =
                null;

            TestLog.Write(
                $"[PARK] Instruction {instruction.Id}");

            var result =
                await engine.ExecuteInstructionAsync<int>(
                        async (
                            instructionProcessor,
                            cancellationToken) =>
                        {
                            Assert.Same(
                                component,
                                instructionProcessor.Component);

                            observedInstructionRegistry =
                                instructionProcessor.Registry;

                            Assert.Same(
                                component,
                                observedInstructionRegistry.Component);

                            Assert.True(
                                observedInstructionRegistry.Register(
                                    instruction));

                            Assert.True(
                                observedInstructionRegistry.TryGet(
                                    instruction.Id,
                                    out var parkedInstruction));

                            Assert.Same(
                                instruction,
                                parkedInstruction);

                            TestLog.Write(
                                $"[READ] Instruction {instruction.Id}");

                            try
                            {
                                using var taskLease =
                                    await instructionProcessor
                                        .RentTaskProcessorAsync(
                                            cancellationToken)
                                        .ConfigureAwait(false);

                                observedTaskProcessor =
                                    taskLease.Instance;

                                observedTaskRegistry =
                                    observedTaskProcessor.Registry;

                                Assert.NotNull(
                                    observedTaskProcessor);

                                Assert.NotNull(
                                    observedTaskRegistry);

                                Assert.Same(
                                    component,
                                    observedTaskProcessor.Component);

                                Assert.Same(
                                    component,
                                    observedTaskRegistry.Component);

                                TestLog.State(
                                    "Instruction processor delegated " +
                                    "to task processor");

                                return 10;
                            }
                            finally
                            {
                                Assert.True(
                                    observedInstructionRegistry.Remove(
                                        instruction.Id));

                                TestLog.Write(
                                    $"[REMOVE] Instruction {instruction.Id}");
                            }
                        })
                    .ConfigureAwait(false);

            Assert.Equal(
                10,
                result);

            Assert.NotNull(
                observedInstructionRegistry);

            Assert.NotNull(
                observedTaskProcessor);

            Assert.NotNull(
                observedTaskRegistry);

            Assert.False(
                observedInstructionRegistry.TryGet(
                    instruction.Id,
                    out _));

            Assert.Equal(
                0,
                engine.Instructions.ActiveCount);

            Assert.Equal(
                1,
                engine.Instructions.AvailableCount);

            TestLog.State(
                "Successful route complete | " +
                $"Active={engine.Instructions.ActiveCount}, " +
                $"Available={engine.Instructions.AvailableCount}, " +
                $"Created={engine.Instructions.CreatedCount}");
        }

        private static async Task
            ProcessingFailureReturnsProcessorsAndRemovesInstruction()
        {
            var component =
                CreateComponent(
                    "certification.end-to-end.failure");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var instruction =
                new SetContextVariableInstruction(
                    "PlayerHealth",
                    10);

            IInstructionsRegistry? observedInstructionRegistry =
                null;

            ITaskRegistry? observedTaskRegistry =
                null;

            TestLog.Write(
                $"[PARK] Failure instruction {instruction.Id}");

            await Assert.ThrowsAsync<ExpectedProcessingException>(
                async () =>
                {
                    await engine.ExecuteInstructionAsync<int>(
                            async (
                                instructionProcessor,
                                cancellationToken) =>
                            {
                                Assert.Same(
                                    component,
                                    instructionProcessor.Component);

                                observedInstructionRegistry =
                                    instructionProcessor.Registry;

                                Assert.Same(
                                    component,
                                    observedInstructionRegistry.Component);

                                Assert.True(
                                    observedInstructionRegistry.Register(
                                        instruction));

                                try
                                {
                                    using var taskLease =
                                        await instructionProcessor
                                            .RentTaskProcessorAsync(
                                                cancellationToken)
                                            .ConfigureAwait(false);

                                    Assert.Same(
                                        component,
                                        taskLease.Instance.Component);

                                    observedTaskRegistry =
                                        taskLease.Instance.Registry;

                                    Assert.NotNull(
                                        observedTaskRegistry);

                                    Assert.Same(
                                        component,
                                        observedTaskRegistry.Component);

                                    throw new ExpectedProcessingException();
                                }
                                finally
                                {
                                    Assert.True(
                                        observedInstructionRegistry.Remove(
                                            instruction.Id));

                                    TestLog.Write(
                                        $"[REMOVE] Failure instruction " +
                                        $"{instruction.Id}");
                                }
                            })
                        .ConfigureAwait(false);
                });

            TestLog.Expected(
                nameof(ExpectedProcessingException));

            Assert.NotNull(
                observedInstructionRegistry);

            Assert.NotNull(
                observedTaskRegistry);

            Assert.False(
                observedInstructionRegistry.TryGet(
                    instruction.Id,
                    out _));

            Assert.Equal(
                0,
                engine.Instructions.ActiveCount);

            Assert.Equal(
                1,
                engine.Instructions.AvailableCount);

            TestLog.State(
                "Failure cleanup complete | " +
                $"Active={engine.Instructions.ActiveCount}, " +
                $"Available={engine.Instructions.AvailableCount}");

            var recoveryResult =
                await ExecuteRecoveryAsync(
                        engine,
                        component)
                    .ConfigureAwait(false);

            Assert.Equal(
                42,
                recoveryResult);

            Assert.Equal(
                0,
                engine.Instructions.ActiveCount);

            TestLog.State(
                "Engine recovered after processing failure");
        }

        private static async Task
            ProcessingCancellationReturnsProcessorsAndRemovesInstruction()
        {
            var component =
                CreateComponent(
                    "certification.end-to-end.cancellation");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var instruction =
                new SetContextVariableInstruction(
                    "PlayerHealth",
                    10);

            IInstructionsRegistry? observedInstructionRegistry =
                null;

            ITaskRegistry? observedTaskRegistry =
                null;

            var enteredProcessing =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            using var cancellation =
                new CancellationTokenSource();

            TestLog.Write(
                $"[PARK] Cancellation instruction {instruction.Id}");

            var processing =
                engine.ExecuteInstructionAsync<int>(
                        async (
                            instructionProcessor,
                            cancellationToken) =>
                        {
                            Assert.Same(
                                component,
                                instructionProcessor.Component);

                            observedInstructionRegistry =
                                instructionProcessor.Registry;

                            Assert.Same(
                                component,
                                observedInstructionRegistry.Component);

                            Assert.True(
                                observedInstructionRegistry.Register(
                                    instruction));

                            try
                            {
                                using var taskLease =
                                    await instructionProcessor
                                        .RentTaskProcessorAsync(
                                            cancellationToken)
                                        .ConfigureAwait(false);

                                Assert.Same(
                                    component,
                                    taskLease.Instance.Component);

                                observedTaskRegistry =
                                    taskLease.Instance.Registry;

                                Assert.NotNull(
                                    observedTaskRegistry);

                                Assert.Same(
                                    component,
                                    observedTaskRegistry.Component);

                                enteredProcessing.TrySetResult(
                                    true);

                                await Task.Delay(
                                        Timeout.Infinite,
                                        cancellationToken)
                                    .ConfigureAwait(false);

                                return 10;
                            }
                            finally
                            {
                                Assert.True(
                                    observedInstructionRegistry.Remove(
                                        instruction.Id));

                                TestLog.Write(
                                    $"[REMOVE] Cancellation instruction " +
                                    $"{instruction.Id}");
                            }
                        },
                        cancellation.Token)
                    .AsTask();

            await enteredProcessing.Task
                .ConfigureAwait(false);

            TestLog.Write(
                "[TASK] Cancelling active processing route");

            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () =>
                {
                    await processing
                        .ConfigureAwait(false);
                });

            TestLog.Expected(
                nameof(OperationCanceledException));

            Assert.NotNull(
                observedInstructionRegistry);

            Assert.NotNull(
                observedTaskRegistry);

            Assert.False(
                observedInstructionRegistry.TryGet(
                    instruction.Id,
                    out _));

            Assert.Equal(
                0,
                engine.Instructions.ActiveCount);

            Assert.Equal(
                1,
                engine.Instructions.AvailableCount);

            TestLog.State(
                "Cancellation cleanup complete | " +
                $"Active={engine.Instructions.ActiveCount}, " +
                $"Available={engine.Instructions.AvailableCount}");

            var recoveryResult =
                await ExecuteRecoveryAsync(
                        engine,
                        component)
                    .ConfigureAwait(false);

            Assert.Equal(
                42,
                recoveryResult);

            Assert.Equal(
                0,
                engine.Instructions.ActiveCount);

            TestLog.State(
                "Engine recovered after processing cancellation");
        }

        private static async Task<int> ExecuteRecoveryAsync(
            Core.WildsEngine engine,
            IComponent component)
        {
            return await engine.ExecuteInstructionAsync<int>(
                    async (
                        instructionProcessor,
                        cancellationToken) =>
                    {
                        Assert.Same(
                            component,
                            instructionProcessor.Component);

                        Assert.Same(
                            component,
                            instructionProcessor.Registry.Component);

                        using var taskLease =
                            await instructionProcessor
                                .RentTaskProcessorAsync(
                                    cancellationToken)
                                .ConfigureAwait(false);

                        Assert.NotNull(
                            taskLease.Instance);

                        Assert.NotNull(
                            taskLease.Instance.Registry);

                        Assert.Same(
                            component,
                            taskLease.Instance.Component);

                        Assert.Same(
                            component,
                            taskLease.Instance.Registry.Component);

                        return 42;
                    })
                .ConfigureAwait(false);
        }

        private static Core.WildsEngine CreateEngine(
            IComponent component)
        {
            return new Core.WildsEngine(
                component,
                CreateOptions());
        }

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static WildsEngineOptions CreateOptions()
        {
            return new WildsEngineOptions
            {
                Layer =
                    "certification.end-to-end",

                RetainedInstructionsProcessors =
                    1,

                ConcurrentInstructionsProcessors =
                    1,

                RetainedTaskProcessors =
                    1,

                ConcurrentTaskProcessors =
                    1
            };
        }

        private static async Task RunAsync(
            string name,
            Func<Task> test)
        {
            TestLog.Test(
                name);

            try
            {
                await test()
                    .ConfigureAwait(false);

                Passed.Add(
                    name);

                TestLog.Pass(
                    name);
            }
            catch (Exception exception)
            {
                TestLog.Fail(
                    name,
                    exception);

                throw;
            }
        }

        private sealed class ExpectedProcessingException :
            Exception
        {
        }
    }
}