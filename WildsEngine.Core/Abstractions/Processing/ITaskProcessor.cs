using System;

namespace WildsEngine.Core.Abstractions
{
    public interface ITaskProcessor :
        IBaseObject
    {
        ITaskRegistry Registry { get; }

        bool RegisterTask(
            ITask task);

        bool ContainsTask(
            Guid id);

        ITask? GetTask(
            Guid id);

        bool TryGetTask(
            Guid id,
            out ITask? task);

        bool RemoveTask(
            Guid id);
    }
}