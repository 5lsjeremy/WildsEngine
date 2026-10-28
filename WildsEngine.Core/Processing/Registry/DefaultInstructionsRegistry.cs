using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing;

public sealed class DefaultInstructionsRegistry :
    InstructionsRegistryBase
{
    public DefaultInstructionsRegistry(string layer)
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