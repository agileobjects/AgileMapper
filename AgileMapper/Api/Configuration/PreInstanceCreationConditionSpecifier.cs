namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<TSource, TTarget, ITypedObjectMappingContext<TSource, TTarget, TInstance>>
    {
        internal PreInstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}