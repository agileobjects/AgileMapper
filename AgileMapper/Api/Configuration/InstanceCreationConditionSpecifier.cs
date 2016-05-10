namespace AgileObjects.AgileMapper.Api.Configuration
{
    using DataSources;
    using ObjectPopulation;

    public class InstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<IInstanceCreationContext<TSource, TTarget, TInstance>>
    {
        internal InstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}