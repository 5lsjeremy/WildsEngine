namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents an object whose transient lease state can be cleared
    /// before the object is returned to a pool.
    /// </summary>
    public interface IResettablePoolObject
    {
        bool TryReset();
    }
}