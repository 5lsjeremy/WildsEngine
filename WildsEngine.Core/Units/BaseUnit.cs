using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Provides stable identity for Wilds temporal and
    /// processing units.
    ///
    /// Units intentionally do not inherit from BaseObject.
    /// A unit is not required to own a component, state,
    /// signal, metadata collection, or neighbor graph.
    /// </summary>
    public abstract class BaseUnit :
        IBaseUnit
    {
        protected BaseUnit()
            : this(
                Guid.NewGuid())
        {
        }

        protected BaseUnit(
            Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Wilds units require a non-empty GUID.",
                    nameof(id));
            }

            Id = id;
        }

        public Guid Id { get; }
    }
}
