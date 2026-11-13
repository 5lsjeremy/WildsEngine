using System;
using System.Linq;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Concrete;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityNativeSurfaceReadableTests
    {
        [Fact]
        public void ExactEntityAndConsumerIdentityArePreserved()
        {
            TestLog.Section("E7-A - EXACT ENTITY AND CONSUMER IDENTITY");
            TestLog.Test("Native surface preserves exact Entity and consumer-owned target references.");
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            var surface = new EntityNativeSurface(entity, consumer);
            TestLog.State($"Entity.Id = {entity.Id}");
            TestLog.State($"Entity.Slug = {entity.Slug}");
            Assert.Same(entity, surface.Entity);
            Assert.Same(consumer, surface.Target);
            TestLog.Pass("Surface preserved the exact Entity reference.");
            TestLog.Pass("Surface preserved the exact consumer-owned target.");
        }

        [Fact]
        public void NativeConsumerVocabularyIsCaptured()
        {
            TestLog.Section("E7-A - NATIVE CONSUMER VOCABULARY");
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 42;
            var surface = new EntityNativeSurface(entity, consumer);
            TestLog.State($"Captured attributes = {surface.NativeAttributes.Count}");
            foreach (var attribute in surface.NativeAttributes)
            {
                TestLog.State($"{attribute.Name} | Type={attribute.ValueType.Name} | InitialValue={FormatValue(attribute.InitialValue)} | CanRead={attribute.CanRead} | CanWrite={attribute.CanWrite}");
            }
            Assert.NotEmpty(surface.NativeAttributes);
            Assert.Contains(surface.NativeAttributes, a => a.Name == nameof(TestConsumer.Health));
            Assert.Contains(surface.NativeAttributes, a => a.Name == nameof(TestConsumer.PublicField));
            TestLog.Pass("Consumer vocabulary was captured without requiring Wilds-specific members on the consumer object.");
        }

        [Fact]
        public void SnapshotAndLiveConsumerAuthorityRemainDistinct()
        {
            TestLog.Section("E7-A - SNAPSHOT VS LIVE CONSUMER AUTHORITY");
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 10;
            var surface = new EntityNativeSurface(entity, consumer);
            var health = RequireAttribute(surface, nameof(TestConsumer.Health));
            var mana = RequireAttribute(surface, nameof(TestConsumer.Mana));
            var name = RequireAttribute(surface, nameof(TestConsumer.Name));
            var field = RequireAttribute(surface, nameof(TestConsumer.PublicField));
            TestLog.State($"Snapshot: Health={FormatValue(health.InitialValue)}, Mana={FormatValue(mana.InitialValue)}, Name={FormatValue(name.InitialValue)}, PublicField={FormatValue(field.InitialValue)}");
            consumer.Health = 22.0;
            consumer.Mana = 9.0;
            consumer.Name = "Changed";
            consumer.PublicField = 99;
            TestLog.State($"Live consumer: Health={consumer.Health}, Mana={consumer.Mana}, Name={consumer.Name}, PublicField={consumer.PublicField}");
            TestLog.State($"Historical snapshot: Health={FormatValue(health.InitialValue)}, Mana={FormatValue(mana.InitialValue)}, Name={FormatValue(name.InitialValue)}, PublicField={FormatValue(field.InitialValue)}");
            Assert.Same(consumer, surface.Target);
            Assert.Equal(100.0, health.InitialValue);
            Assert.Equal(50.0, mana.InitialValue);
            Assert.Equal("Mara", name.InitialValue);
            Assert.Equal(10, field.InitialValue);
            TestLog.Pass("Native snapshot remained historical after live mutation.");
            TestLog.Pass("The exact consumer object remained the live authority.");
        }

        [Fact]
        public void RepeatedNativeCaptureIsDeterministic()
        {
            TestLog.Section("E7-A - DETERMINISTIC NATIVE CAPTURE");
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 42;
            var first = new EntityNativeSurface(entity, consumer);
            var second = new EntityNativeSurface(entity, consumer);
            TestLog.State($"First capture count = {first.NativeAttributes.Count}");
            TestLog.State($"Second capture count = {second.NativeAttributes.Count}");
            Assert.Equal(first.NativeAttributes.Count, second.NativeAttributes.Count);
            for (var index = 0; index < first.NativeAttributes.Count; index++)
            {
                var left = first.NativeAttributes[index];
                var right = second.NativeAttributes[index];
                TestLog.State($"[{index}] {left.Name} | First={FormatValue(left.InitialValue)} | Second={FormatValue(right.InitialValue)}");
                Assert.Equal(left.Name, right.Name);
                Assert.Equal(left.ValueType, right.ValueType);
                Assert.Equal(left.InitialValue, right.InitialValue);
                Assert.Equal(left.CanRead, right.CanRead);
                Assert.Equal(left.CanWrite, right.CanWrite);
            }
            TestLog.Pass("Repeated capture preserved vocabulary order and values.");
        }

        private static TestConsumer CreateConsumer() =>
            new TestConsumer(100.0, 50.0, 75.0, "Mara");

        private static INativeAttribute RequireAttribute(IEntityNativeSurface surface, string name)
        {
            var matches = surface.NativeAttributes.Where(a => string.Equals(a.Name, name, StringComparison.Ordinal)).ToArray();
            Assert.Single(matches);
            return matches[0];
        }

        private static string FormatValue(object? value) =>
            value?.ToString() ?? "<null>";
    }
}
