namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public class ConditionSpecifier<TSource, TTarget>
        : ConditionSpecifierBase<TSource, TTarget, ITypedMemberMappingContext<TSource, TTarget>>
    {
        internal ConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }

        public void If(Expression<Func<TSource, TTarget, int?, bool>> condition) => AddConditionFactory(condition);
    }
}