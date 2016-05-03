namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    public class ConditionSpecifier<TSource, TTarget>
    {
        private readonly UserConfiguredItemBase _configuredItem;
        private readonly bool _negateCondition;

        internal ConditionSpecifier(
            UserConfiguredItemBase configuredItem,
            bool negateCondition)
        {
            _configuredItem = configuredItem;
            _negateCondition = negateCondition;
        }

        public void If(Expression<Func<TSource, bool>> condition)
        {
            AddCondition(condition, context => new[] { context.SourceObject });
        }

        public void If(Expression<Func<TSource, TTarget, bool>> condition)
        {
            AddCondition(condition, context => new[] { context.SourceObject, context.TargetVariable });
        }

        public void If(Expression<Func<TSource, TTarget, int?, bool>> condition)
        {
            AddCondition(
                condition,
                context => new[] { context.SourceObject, context.TargetVariable, context.EnumerableIndex });
        }

        private void AddCondition(
            LambdaExpression condition,
            Func<IConfigurationContext, Expression[]> parameterReplacementsFactory)
        {
            _configuredItem.AddCondition(context =>
            {
                var parameterReplacements = parameterReplacementsFactory.Invoke(context);
                var contextualisedCondition = condition.ReplaceParameters(parameterReplacements);

                if (_negateCondition)
                {
                    contextualisedCondition = Expression.Not(contextualisedCondition);
                }

                return contextualisedCondition;
            });
        }
    }
}