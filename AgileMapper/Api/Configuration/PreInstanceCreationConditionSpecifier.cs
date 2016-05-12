namespace AgileObjects.AgileMapper.Api.Configuration
{
    using DataSources;
    using ObjectPopulation;

    public class PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<IInstanceCreationContext<TSource, TTarget, TInstance>>
    {
        internal PreInstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}