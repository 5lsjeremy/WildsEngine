using System;
using System.Collections.Generic;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Represents one complete QSL request submitted
    /// through the Wilds processing lifecycle.
    ///
    /// Wilds treats this request as opaque runtime data.
    /// The Wilds/QSL integration layer prepares QSL from
    /// the supplied consumer bindings, state packages,
    /// and source.
    /// </summary>
    public sealed class WildsQslRequest
    {
        private readonly QslConsumerBinding[] _consumers;

        private readonly QslStatePackage[] _statePackages;

        public WildsQslRequest(
            string source,
            IEnumerable<QslConsumerBinding>? consumers = null,
            IEnumerable<QslStatePackage>? statePackages = null)
        {
            if (string.IsNullOrWhiteSpace(
                    source))
            {
                throw new ArgumentException(
                    "QSL source cannot be empty.",
                    nameof(source));
            }

            Source =
                source;

            _consumers =
                Copy(
                    consumers,
                    nameof(consumers));

            _statePackages =
                Copy(
                    statePackages,
                    nameof(statePackages));
        }

        public string Source { get; }

        public IReadOnlyList<QslConsumerBinding> Consumers =>
            _consumers;

        public IReadOnlyList<QslStatePackage> StatePackages =>
            _statePackages;

        private static T[] Copy<T>(
            IEnumerable<T>? values,
            string parameterName)
            where T : class
        {
            if (values == null)
            {
                return Array.Empty<T>();
            }

            var items =
                new List<T>();

            foreach (var value in values)
            {
                if (value == null)
                {
                    throw new ArgumentException(
                        "Request collections cannot contain null.",
                        parameterName);
                }

                items.Add(
                    value);
            }

            return items.ToArray();
        }
    }
}