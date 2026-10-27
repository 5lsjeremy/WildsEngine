using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Tests;

public sealed class TestSignal :
    BaseObject,
    ISignal
{
    private readonly IComponent _component;

    public TestSignal(
        IComponent component,
        string layer)
        : base(layer)
    {
        _component = component
                     ?? throw new ArgumentNullException(
                         nameof(component));
    }

    public override IComponent Component =>
        _component;
}