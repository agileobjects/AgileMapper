namespace AgileObjects.AgileMapper.Api.Configuration
{
    using DataSources;
    using Members;

    public class ConditionSpecifier<TSource, TTarget> : ConditionSpecifierBase<ITypedMemberMappingContext<TSource, TTarget>>
    {
        internal ConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }
    }
}