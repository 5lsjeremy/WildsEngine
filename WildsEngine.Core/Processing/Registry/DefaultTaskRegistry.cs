using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;

public sealed class DefaultTaskRegistry :
    TaskRegistryBase
{
    public DefaultTaskRegistry(string layer)
        : base(layer)
    {
    }

    protected override void OnInitialize()
    {
    }

    protected override void OnShutdown()
    {
    }

    public override IComponent Component { get; }
}