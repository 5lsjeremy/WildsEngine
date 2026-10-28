using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Tests;

public sealed class UnknownInstructionBase :
    StateInstructionBase
{
    public UnknownInstructionBase()
        : base(
            intent: "test.unknown",
            payload: null)
    {
    }
}