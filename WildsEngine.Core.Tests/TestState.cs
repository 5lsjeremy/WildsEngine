using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;
using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Tests;

public sealed class TestState :
    BaseObject,
    IState
{
    private readonly IComponent _component;

    public TestState(
        IComponent component,
        IContext context,
        string layer)
        : base(layer)
    {
        _component = component
                     ?? throw new ArgumentNullException(
                         nameof(component));

        Context = context
                  ?? throw new ArgumentNullException(
                      nameof(context));
    }

    public override IComponent Component =>
        _component;

    public IContext Context { get; }

    public bool Process(
        IStateInstruction instruction)
    {
        if (instruction == null)
        {
            throw new ArgumentNullException(
                nameof(instruction));
        }

        if (instruction is not ISetContextVariableInstruction setVariable)
        {
            return false;
        }

        Context.SetVariable(
            setVariable.Key,
            setVariable.Value);

        return true;
    }
}