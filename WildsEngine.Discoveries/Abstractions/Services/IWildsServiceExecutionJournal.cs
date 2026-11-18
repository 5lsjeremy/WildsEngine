using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceExecutionJournal
    {
        void Record(IWildsServiceExecutionRecord record);
        bool TryGet(Guid executionId, out IWildsServiceExecutionRecord? record);
        IReadOnlyList<IWildsServiceExecutionRecord> GetByService(Guid serviceId);
        IReadOnlyList<IWildsServiceExecutionRecord> GetByPulse(Guid pulseId);
    }
}
