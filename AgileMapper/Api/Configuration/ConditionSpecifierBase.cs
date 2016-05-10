namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    public abstract class ConditionSpecifierBase<TContext>
    {
        private readonly UserConfiguredItemBase _configuredItem;
        private readonly bool _negateCondition;

        internal ConditionSpecifierBase(UserConfiguredItemBase configuredItem, bool negateCondition)
        {
            _configuredItem = configuredItem;
            _negateCondition = negateCondition;
        }

        public void If(Expression<Func<TContext, bool>> condition)
        {
            _configuredItem.AddConditionFactory(contextParameter =>
            {
                var contextualisedCondition = condition.ReplaceParameterWith(contextParameter);

                if (_negateCondition)
                {
                    contextualisedCondition = Expression.Not(contextualisedCondition);
                }

                return contextualisedCondition;
            });
        }
    }
}