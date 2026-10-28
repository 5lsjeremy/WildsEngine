using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Core.Abstractions
{
    public interface IInstructionsProcessor :
        IBaseObject
    {
        IInstructionsRegistry Registry { get; }

        ValueTask<ObjectLease<ITaskProcessor>>
            RentTaskProcessorAsync(
                CancellationToken cancellationToken = default);
    }
}