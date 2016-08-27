namespace AgileObjects.AgileMapper.Api.Configuration
{
    public interface IConditionalRootMappingConfigurator<TSource, TTarget>
        : IRootMappingConfigurator<TSource, TTarget>
    {
        MappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget;
    }
}