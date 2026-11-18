namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorMotionEvaluatorFactory
    {
        IDiscoveryVectorMotionEvaluator Create(IDiscoveryVectorMotionService service);
    }
}
