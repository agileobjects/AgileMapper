namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public class PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>
        : ConditionSpecifierBase<TSource, TTarget, ITypedObjectMappingContext<TSource, TTarget, TInstance>>
    {
        internal PostInstanceCreationConditionSpecifier(UserConfiguredItemBase configuredItem, bool negateCondition = false)
            : base(configuredItem, negateCondition)
        {
        }

        public void If(Expression<Func<TSource, TTarget, TInstance, int?, bool>> condition) => AddConditionFactory(condition);
    }
}