using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborPipeline : NeighborCompositeOperatorBase
    {
        public NeighborPipeline(params INeighborSequenceOperator[] operators)
            : base(operators ?? throw new ArgumentNullException(nameof(operators))) { }

        public NeighborPipeline(IReadOnlyList<INeighborSequenceOperator> operators)
            : base(operators) { }
    }
}
