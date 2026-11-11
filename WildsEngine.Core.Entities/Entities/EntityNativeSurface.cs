using System;
using System.Collections.Generic;
using System.Reflection;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Concrete
{
    public sealed class EntityNativeSurface :
        IEntityNativeSurface
    {
        private readonly IReadOnlyList<INativeAttribute> _nativeAttributes;

        public EntityNativeSurface(
            IEntity entity,
            object target)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            Target =
                target ??
                throw new ArgumentNullException(
                    nameof(target));

            _nativeAttributes =
                Capture(
                    target);
        }

        public IEntity Entity { get; }

        public object Target { get; }

        public IReadOnlyList<INativeAttribute> NativeAttributes =>
            _nativeAttributes;

        private static IReadOnlyList<INativeAttribute> Capture(
            object target)
        {
            var attributes =
                new List<INativeAttribute>();

            var properties =
                target.GetType().GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            Array.Sort(
                properties,
                (left, right) =>
                    string.CompareOrdinal(
                        left.Name,
                        right.Name));

            for (var index = 0;
                 index < properties.Length;
                 index++)
            {
                var property =
                    properties[index];

                if (property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                var canRead =
                    property.GetMethod != null &&
                    property.GetMethod.IsPublic;

                if (!canRead)
                {
                    continue;
                }

                var canWrite =
                    property.SetMethod != null &&
                    property.SetMethod.IsPublic;

                attributes.Add(
                    new NativeAttribute(
                        property.Name,
                        property.PropertyType,
                        property.GetValue(target),
                        canRead,
                        canWrite));
            }

            var fields =
                target.GetType().GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            Array.Sort(
                fields,
                (left, right) =>
                    string.CompareOrdinal(
                        left.Name,
                        right.Name));

            for (var index = 0;
                 index < fields.Length;
                 index++)
            {
                var field =
                    fields[index];

                attributes.Add(
                    new NativeAttribute(
                        field.Name,
                        field.FieldType,
                        field.GetValue(target),
                        true,
                        !field.IsInitOnly && !field.IsLiteral));
            }

            return attributes;
        }
    }
}
