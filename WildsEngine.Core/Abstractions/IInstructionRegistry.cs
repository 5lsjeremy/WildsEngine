using System;
using WildsEngine.Core.Abstractions;

public interface IInstructionRegistry :
    IBaseObject
{
    bool Register(IStateInstruction instruction);

    bool TryGet(
        Guid id,
        out IStateInstruction? instruction);

    bool Remove(Guid id);
}