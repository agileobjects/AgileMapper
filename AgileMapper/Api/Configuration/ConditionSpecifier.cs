namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    public class ConditionSpecifier<TSource, TTarget>
    {
        private readonly UserConfiguredItemBase _configuredItem;

        internal ConditionSpecifier(UserConfiguredItemBase configuredItem)
        {
            _configuredItem = configuredItem;
        }

        public void If(Expression<Func<TSource, TTarget, bool>> condition)
        {
            _configuredItem.AddCondition(context =>
            {
                var contextualisedCondition = condition
                    .ReplaceParameters(context.SourceObject, context.ExistingObject);

                return Expression.Not(contextualisedCondition);
            });
        }
    }
}