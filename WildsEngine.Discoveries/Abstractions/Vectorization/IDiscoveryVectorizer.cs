using System;
using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Discoveries.Abstractions.Vectorization
{
    public interface IDiscoveryVectorizer
    {
        Task<IDiscoveryVectorizationResult> VectorizeAsync(
            IDiscoveryVectorInput input,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default);
    }
}
