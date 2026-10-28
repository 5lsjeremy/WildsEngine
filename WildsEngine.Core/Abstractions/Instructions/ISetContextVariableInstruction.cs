namespace WildsEngine.Core.Abstractions
{
    public interface ISetContextVariableInstruction :
        IStateInstruction
    {
        string Key { get; }

        object? Value { get; }
    }
}