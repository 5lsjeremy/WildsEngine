using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;
using WildsEngine.Core.Processing;

namespace WildsEngine.Core.States
{
    public abstract class WildsState :
        BaseObject,
        IState
    {
        private readonly IComponent _component;

        protected WildsState(
            IComponent component,
            IContext context,
            string layer)
            : base(layer)
        {
            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));

            Context =
                context ??
                throw new ArgumentNullException(
                    nameof(context));
        }

        protected WildsState(
            Guid id,
            IComponent component,
            IContext context,
            string layer)
            : base(
                id,
                layer)
        {
            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));

            Context =
                context ??
                throw new ArgumentNullException(
                    nameof(context));
        }

        public sealed override IComponent Component =>
            _component;

        public IContext Context { get; }

        public abstract bool Process(
            IStateInstruction instruction);

        /// <summary>
        /// Processes Neighbor-topology instructions that are
        /// common to all Wilds states.
        ///
        /// Concrete states should invoke this before handling
        /// state-specific instruction types.
        /// </summary>
        protected bool TryProcessNeighborInstruction(
            IStateInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(
                    nameof(instruction));
            }

            if (instruction is
                AddNeighborInstruction addNeighbor)
            {
                return Component.Neighbors.Add(
                    addNeighbor.Neighbor,
                    addNeighbor.Membership,
                    addNeighbor.Distance);
            }

            if (instruction is
                RemoveNeighborInstruction removeNeighbor)
            {
                return Component.Neighbors.Remove(
                    removeNeighbor.NeighborId,
                    removeNeighbor.Membership);
            }

            if (instruction is
                SetNeighborDistanceInstruction setDistance)
            {
                return Component.Neighbors.SetDistance(
                    setDistance.NeighborId,
                    setDistance.Membership,
                    setDistance.Distance);
            }

            return false;
        }
    }
}