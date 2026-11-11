using System;
using System.Linq;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Concrete;
using WildsEngine.Core.Tests.Entities.Fixtures;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityNativeSurfaceCertificationTests
    {
        [Fact]
        public void NativeSurfacePreservesExactEntityAndConsumerTarget()
        {
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            var surface = new EntityNativeSurface(entity, consumer);
            Assert.Same(entity, surface.Entity);
            Assert.Same(consumer, surface.Target);
        }

        [Fact]
        public void NativeSurfaceCapturesReadablePublicProperties()
        {
            var surface = CreateSurface();
            var health = RequireAttribute(surface, nameof(TestConsumer.Health));
            var mana = RequireAttribute(surface, nameof(TestConsumer.Mana));
            var stamina = RequireAttribute(surface, nameof(TestConsumer.Stamina));
            var name = RequireAttribute(surface, nameof(TestConsumer.Name));

            Assert.Equal(typeof(double), health.ValueType);
            Assert.Equal(100.0, health.InitialValue);
            Assert.True(health.CanRead);
            Assert.True(health.CanWrite);
            Assert.Equal(typeof(double), mana.ValueType);
            Assert.Equal(50.0, mana.InitialValue);
            Assert.True(mana.CanRead);
            Assert.True(mana.CanWrite);
            Assert.Equal(typeof(double), stamina.ValueType);
            Assert.Equal(75.0, stamina.InitialValue);
            Assert.True(stamina.CanRead);
            Assert.False(stamina.CanWrite);
            Assert.Equal(typeof(string), name.ValueType);
            Assert.Equal("Mara", name.InitialValue);
            Assert.True(name.CanRead);
            Assert.True(name.CanWrite);
        }

        [Fact]
        public void NativeSurfaceCapturesPublicFields()
        {
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 42;
            var surface = new EntityNativeSurface(entity, consumer);
            var field = RequireAttribute(surface, nameof(TestConsumer.PublicField));
            Assert.Equal(typeof(int), field.ValueType);
            Assert.Equal(42, field.InitialValue);
            Assert.True(field.CanRead);
            Assert.True(field.CanWrite);
        }

        [Fact]
        public void NativeSurfaceCapturesCalculatedReadOnlyProperties()
        {
            var surface = CreateSurface();
            var label = RequireAttribute(surface, nameof(TestConsumer.ReadOnlyLabel));
            Assert.Equal(typeof(string), label.ValueType);
            Assert.Equal("Mara", label.InitialValue);
            Assert.True(label.CanRead);
            Assert.False(label.CanWrite);
        }

        [Fact]
        public void NativeAttributeNamesAndValuesMatchConsumerVocabulary()
        {
            var entity = new TestEntity("mara");
            var consumer = new TestConsumer(88.0, 37.0, 64.0, "Mara");
            consumer.PublicField = 12;
            var surface = new EntityNativeSurface(entity, consumer);
            Assert.Equal(88.0, RequireAttribute(surface, "Health").InitialValue);
            Assert.Equal(37.0, RequireAttribute(surface, "Mana").InitialValue);
            Assert.Equal(64.0, RequireAttribute(surface, "Stamina").InitialValue);
            Assert.Equal("Mara", RequireAttribute(surface, "Name").InitialValue);
            Assert.Equal(12, RequireAttribute(surface, "PublicField").InitialValue);
        }

        [Fact]
        public void NativeAttributeSnapshotDoesNotChangeWhenConsumerChanges()
        {
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 10;
            var surface = new EntityNativeSurface(entity, consumer);
            var health = RequireAttribute(surface, nameof(TestConsumer.Health));
            var mana = RequireAttribute(surface, nameof(TestConsumer.Mana));
            var name = RequireAttribute(surface, nameof(TestConsumer.Name));
            var field = RequireAttribute(surface, nameof(TestConsumer.PublicField));
            consumer.Health = 25.0;
            consumer.Mana = 10.0;
            consumer.Name = "Changed";
            consumer.PublicField = 99;
            Assert.Equal(100.0, health.InitialValue);
            Assert.Equal(50.0, mana.InitialValue);
            Assert.Equal("Mara", name.InitialValue);
            Assert.Equal(10, field.InitialValue);
        }

        [Fact]
        public void ConsumerRemainsLiveAuthorityAfterNativeSnapshot()
        {
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            var surface = new EntityNativeSurface(entity, consumer);
            consumer.Health = 22.0;
            consumer.Mana = 9.0;
            Assert.Same(consumer, surface.Target);
            Assert.Equal(22.0, consumer.Health);
            Assert.Equal(9.0, consumer.Mana);
            Assert.Equal(100.0, RequireAttribute(surface, nameof(TestConsumer.Health)).InitialValue);
            Assert.Equal(50.0, RequireAttribute(surface, nameof(TestConsumer.Mana)).InitialValue);
        }

        [Fact]
        public void NativeSurfaceCaptureIsStableAcrossRepeatedConstruction()
        {
            var entity = new TestEntity("mara");
            var consumer = CreateConsumer();
            consumer.PublicField = 42;
            var first = new EntityNativeSurface(entity, consumer);
            var second = new EntityNativeSurface(entity, consumer);
            Assert.Equal(first.NativeAttributes.Count, second.NativeAttributes.Count);
            for (var index = 0; index < first.NativeAttributes.Count; index++)
            {
                Assert.Equal(first.NativeAttributes[index].Name, second.NativeAttributes[index].Name);
                Assert.Equal(first.NativeAttributes[index].ValueType, second.NativeAttributes[index].ValueType);
                Assert.Equal(first.NativeAttributes[index].InitialValue, second.NativeAttributes[index].InitialValue);
                Assert.Equal(first.NativeAttributes[index].CanRead, second.NativeAttributes[index].CanRead);
                Assert.Equal(first.NativeAttributes[index].CanWrite, second.NativeAttributes[index].CanWrite);
            }
        }

        [Fact]
        public void NativeSurfaceRejectsNullEntity()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EntityNativeSurface(null!, CreateConsumer()));
        }

        [Fact]
        public void NativeSurfaceRejectsNullConsumerTarget()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EntityNativeSurface(new TestEntity("mara"), null!));
        }

        [Fact]
        public void NativeAttributeCollectionContainsUniqueVocabularyNames()
        {
            var surface = CreateSurface();
            var names = surface.NativeAttributes.Select(attribute => attribute.Name).ToArray();
            var distinctNames = names.Distinct(StringComparer.Ordinal).ToArray();
            Assert.Equal(names.Length, distinctNames.Length);
        }

        private static TestConsumer CreateConsumer() =>
            new TestConsumer(100.0, 50.0, 75.0, "Mara");

        private static IEntityNativeSurface CreateSurface() =>
            new EntityNativeSurface(new TestEntity("mara"), CreateConsumer());

        private static INativeAttribute RequireAttribute(
            IEntityNativeSurface surface,
            string name)
        {
            var matches = surface.NativeAttributes
                .Where(attribute => string.Equals(attribute.Name, name, StringComparison.Ordinal))
                .ToArray();
            Assert.Single(matches);
            return matches[0];
        }
    }
}
