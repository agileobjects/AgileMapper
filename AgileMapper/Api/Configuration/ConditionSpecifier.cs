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

        public void If(Expression<Func<TSource, bool>> condition)
        {
            AddCondition(condition, context => new[] { context.SourceObject });
        }

        public void If(Expression<Func<TSource, TTarget, bool>> condition)
        {
            AddCondition(condition, context => new[] { context.SourceObject, context.ExistingObject });
        }

        private void AddCondition(
            LambdaExpression condition,
            Func<IConfigurationContext, Expression[]> parameterReplacementsFactory)
        {
            _configuredItem.AddCondition(context =>
            {
                var parameterReplacements = parameterReplacementsFactory.Invoke(context);
                var contextualisedCondition = condition.ReplaceParameters(parameterReplacements);
                var negatedCondition = Expression.Not(contextualisedCondition);

                return negatedCondition;
            });
        }
    }
}