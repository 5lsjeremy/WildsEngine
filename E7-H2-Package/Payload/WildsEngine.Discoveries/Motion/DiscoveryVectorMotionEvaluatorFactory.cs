using System;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorMotionEvaluatorFactory : IDiscoveryVectorMotionEvaluatorFactory
    {
        public IDiscoveryVectorMotionEvaluator Create(IDiscoveryVectorMotionService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            return new DiscoveryVectorMotionEvaluator(service);
        }
    }
}
