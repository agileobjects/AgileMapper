namespace AgileObjects.AgileMapper.Api.Configuration
{
    using Members;

    public class ConditionSpecifier<TSource, TTarget>
        : ConditionSpecifierBase<TSource, TTarget, ITypedMemberMappingContext<TSource, TTarget>>
    {
        internal ConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}