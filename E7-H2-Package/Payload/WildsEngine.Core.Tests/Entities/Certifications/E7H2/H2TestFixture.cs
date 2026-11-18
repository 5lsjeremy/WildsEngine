using System;
using System.Collections.Generic;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Vectors;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    internal sealed class H2TestFixture
    {
        public H2TestFixture()
        {
            Entity = new TestEntity("h2-owner");
            Discovery = new TestDiscovery(Entity, 3, "H2 Discovery", "h2-discovery");
            Input = new DiscoveryVectorInput(Discovery, "h2-input");
        }
        public TestEntity Entity { get; }
        public TestDiscovery Discovery { get; }
        public DiscoveryVectorInput Input { get; }
        public IDiscoveryVectorEvaluation Eval(DateTimeOffset at, params (string,double)[] values)
        {
            var components = new List<IDiscoveryVectorComponent>();
            var contributions = new List<IDiscoveryVectorContribution>();
            foreach (var value in values)
            {
                components.Add(new DiscoveryVectorComponent(value.Item1, value.Item2));
                contributions.Add(new DiscoveryVectorContribution(Input, value.Item1, value.Item2, 1.0));
            }
            var vector = new DiscoveryVector(components, contributions, new List<IDiscoveryVectorMotion>(), new AbsoluteMetric(), at);
            return new DiscoveryVectorEvaluation(vector, Guid.NewGuid(), at);
        }
        public IDiscoveryVectorEvaluationSeries Series(params IDiscoveryVectorEvaluation[] evaluations) =>
            new DiscoveryVectorEvaluationSeriesFactory().Create("h2-scope", new List<IDiscoveryVectorInput>{Input}, evaluations);
        private sealed class AbsoluteMetric : IDiscoveryVectorMetric
        {
            public double GetMagnitude(IDiscoveryVectorPoint point)
            { var total=0.0; for(var i=0;i<point.Components.Count;i++) total += Math.Abs(point.Components[i].Value); return total; }
        }
    }
}
