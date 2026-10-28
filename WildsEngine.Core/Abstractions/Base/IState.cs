namespace WildsEngine.Core.Abstractions
{
    public interface IState : IBaseObject
    {
        IContext Context { get; }

        bool Process(IStateInstruction instruction);
    }
}
