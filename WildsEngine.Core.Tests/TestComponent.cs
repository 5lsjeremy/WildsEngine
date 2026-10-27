using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Components;

namespace WildsEngine.Core.Tests;

public sealed class TestComponent :
    BaseComponent
{
    private readonly IState _state;
    private readonly ISignal _signal;

    public TestComponent(string layer)
        : base(layer)
    {
        var context = new TestContext(
            this,
            $"{layer}.context");

        _state = new TestState(
            this,
            context,
            $"{layer}.state");

        _signal = new TestSignal(
            this,
            $"{layer}.signal");
    }

    public override IState State => _state;

    public override ISignal Signal => _signal;
}