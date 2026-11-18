using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceExecutionJournal : IWildsServiceExecutionJournal
    {
        private readonly ConcurrentDictionary<Guid, IWildsServiceExecutionRecord> _records =
            new ConcurrentDictionary<Guid, IWildsServiceExecutionRecord>();

        public void Record(IWildsServiceExecutionRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _records.AddOrUpdate(record.Id, record, (_, __) => record);
        }

        public bool TryGet(Guid executionId, out IWildsServiceExecutionRecord? record) =>
            _records.TryGetValue(executionId, out record);

        public IReadOnlyList<IWildsServiceExecutionRecord> GetByService(Guid serviceId) =>
            Filter(record => record.ServiceId == serviceId);

        public IReadOnlyList<IWildsServiceExecutionRecord> GetByPulse(Guid pulseId) =>
            Filter(record => record.PulseId == pulseId);

        private IReadOnlyList<IWildsServiceExecutionRecord> Filter(Func<IWildsServiceExecutionRecord, bool> predicate)
        {
            var result = new List<IWildsServiceExecutionRecord>();
            foreach (var record in _records.Values)
                if (predicate(record)) result.Add(record);
            result.Sort((left, right) => left.RequestedAt.CompareTo(right.RequestedAt));
            return result.AsReadOnly();
        }
    }
}
