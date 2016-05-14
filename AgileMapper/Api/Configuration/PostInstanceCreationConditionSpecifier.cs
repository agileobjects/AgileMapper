namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<TSource, TTarget, IInstanceCreationContext<TSource, TTarget, TInstance>>
    {
        internal PostInstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}