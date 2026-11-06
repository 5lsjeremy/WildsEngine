using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Tests.Fixtures
{
    internal sealed class TestEmissionDataSource :
        IEmissionDataSource
    {
        private readonly IReadOnlyDictionary<
            string,
            object?> _properties;

        private readonly IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<
                string,
                object?>> _groups;

        public TestEmissionDataSource(
            IReadOnlyDictionary<
                string,
                object?>? properties = null,
            IReadOnlyDictionary<
                string,
                IReadOnlyDictionary<
                    string,
                    object?>>? groups = null)
        {
            _properties =
                properties ??
                new Dictionary<
                    string,
                    object?>();

            _groups =
                groups ??
                new Dictionary<
                    string,
                    IReadOnlyDictionary<
                        string,
                        object?>>();
        }

        public bool TryGetProperty(
            string propertyName,
            out object? value)
        {
            return _properties.TryGetValue(
                propertyName,
                out value);
        }

        public bool TryGetGroup(
            string groupName,
            out IReadOnlyDictionary<
                string,
                object?> values)
        {
            if (_groups.TryGetValue(
                    groupName,
                    out var resolved))
            {
                values =
                    resolved;

                return true;
            }

            values =
                new Dictionary<
                    string,
                    object?>();

            return false;
        }
    }
}


