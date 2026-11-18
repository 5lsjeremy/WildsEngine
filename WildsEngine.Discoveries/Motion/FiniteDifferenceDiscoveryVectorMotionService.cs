using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Vectors;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class FiniteDifferenceDiscoveryVectorMotionService : DiscoveryVectorMotionServiceBase
    {
        public FiniteDifferenceDiscoveryVectorMotionService(
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(slug, contexts, journal, clock) { }

        protected override Task<DiscoveryVectorMotionServiceResult> ExecuteCoreAsync(
            DiscoveryVectorMotionRequest request,
            IWildsServiceContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var series = request.Series;
            var count = series.Evaluations.Count;
            var current = series.Evaluations[count - 1];
            if (count < 2)
                return Task.FromResult(new DiscoveryVectorMotionServiceResult(
                    current, new List<IDiscoveryVectorMotion>()));

            var previous = series.Evaluations[count - 2];
            var dt = Seconds(previous.EvaluatedAt, current.EvaluatedAt);
            var prior = count >= 3 ? series.Evaluations[count - 3] : null;
            var priorDt = prior == null ? 0.0 : Seconds(prior.EvaluatedAt, previous.EvaluatedAt);
            var currentMap = Map(current.Vector);
            var previousMap = Map(previous.Vector);
            var priorMap = prior == null ? null : Map(prior.Vector);
            var motion = new List<IDiscoveryVectorMotion>();

            foreach (var pair in currentMap)
            {
                if (!previousMap.TryGetValue(pair.Key, out var previousValue)) continue;
                var velocity = Finite((pair.Value - previousValue) / dt, "velocity");
                var acceleration = 0.0;
                if (priorMap != null && priorMap.TryGetValue(pair.Key, out var priorValue))
                {
                    var previousVelocity = Finite((previousValue - priorValue) / priorDt, "previous velocity");
                    acceleration = Finite((velocity - previousVelocity) / dt, "acceleration");
                }
                motion.Add(new DiscoveryVectorMotion(pair.Key, velocity, acceleration));
            }
            return Task.FromResult(new DiscoveryVectorMotionServiceResult(current, motion));
        }

        private static double Seconds(DateTimeOffset earlier, DateTimeOffset later)
        {
            var seconds = (later - earlier).TotalSeconds;
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0.0)
                throw new InvalidOperationException("Motion requires strictly increasing evaluation times.");
            return seconds;
        }
        private static SortedDictionary<string,double> Map(IDiscoveryVector vector)
        {
            var map = new SortedDictionary<string,double>(StringComparer.Ordinal);
            for (var i = 0; i < vector.Components.Count; i++)
                map.Add(vector.Components[i].Name, vector.Components[i].Value);
            return map;
        }
        private static double Finite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidOperationException("Derived " + name + " must be finite.");
            return value;
        }
    }
}
