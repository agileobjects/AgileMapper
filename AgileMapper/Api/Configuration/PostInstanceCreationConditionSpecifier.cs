namespace AgileObjects.AgileMapper.Api.Configuration
{
    using DataSources;
    using ObjectPopulation;

    public class PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<IInstanceCreationContext<TSource, TTarget, TInstance>>
    {
        internal PostInstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}