using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.States;

namespace WildsEngine.Tests.Registry;

internal sealed class TestState :
    WildsState
{
    public TestState(
        IComponent component,
        IContext context,
        string layer)
        : base(
            component,
            context,
            layer)
    {
    }

    public override bool Process(
        IStateInstruction instruction)
    {
        if (instruction == null)
        {
            throw new ArgumentNullException(
                nameof(instruction));
        }

        if (instruction is
            ISetContextVariableInstruction
            setContextVariable)
        {
            Context.SetVariable(
                setContextVariable.Key,
                setContextVariable.Value);

            return true;
        }

        return false;
    }
}