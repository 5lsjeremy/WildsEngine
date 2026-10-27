using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Tests;

public sealed class UnknownInstruction :
    StateInstruction
{
    public UnknownInstruction()
        : base(
            intent: "test.unknown",
            payload: null)
    {
    }
}