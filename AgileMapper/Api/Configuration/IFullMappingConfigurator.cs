namespace AgileObjects.AgileMapper.Api.Configuration
{
    public interface IFullMappingConfigurator<TSource, TTarget> : IConditionalMappingConfigurator<TSource, TTarget>
    {
        PreEventMappingConfigStartingPoint<TSource, TTarget> Before { get; }

        PostEventMappingConfigStartingPoint<TSource, TTarget> After { get; }

        DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget> Map<TDerivedSource>()
            where TDerivedSource : TSource;
    }
}