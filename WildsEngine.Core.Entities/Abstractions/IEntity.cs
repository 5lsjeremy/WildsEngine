using System.Collections.Generic;
using WildsEngine.Core.Entities.Abstractions;

public interface IEntity :
    IIdentifier
{
    IReadOnlyList<IFact> Facts { get; }
}