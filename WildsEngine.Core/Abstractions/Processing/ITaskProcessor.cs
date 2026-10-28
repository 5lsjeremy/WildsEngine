namespace WildsEngine.Core.Abstractions
{
    public interface ITaskProcessor :
        IBaseObject
    {
        ITaskRegistry Registry { get; }
    }
}