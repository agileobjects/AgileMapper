namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public abstract class ConditionSpecifierBase<TSource, TTarget, TContext>
        where TContext : ITypedMemberMappingContext<TSource, TTarget>
    {
        private readonly UserConfiguredItemBase _configuredItem;
        private readonly bool _negateCondition;

        internal ConditionSpecifierBase(UserConfiguredItemBase configuredItem, bool negateCondition)
        {
            _configuredItem = configuredItem;
            _negateCondition = negateCondition;
        }

        public void If(Expression<Func<TContext, bool>> condition) => AddConditionFactory(condition);

        public void If(Expression<Func<TSource, TTarget, bool>> condition) => AddConditionFactory(condition);

        public void If(Expression<Func<TSource, TTarget, int?, bool>> condition) => AddConditionFactory(condition);

        protected void AddConditionFactory(LambdaExpression conditionLambda)
        {
            _configuredItem.AddConditionFactory(context =>
            {
                var lambdaInfo = ConfiguredLambdaInfo.For(conditionLambda);
                var contextualisedCondition = lambdaInfo.GetBody(context);

                if (_negateCondition)
                {
                    contextualisedCondition = Expression.Not(contextualisedCondition);
                }

                return context.GetTryCall(contextualisedCondition);
            });
        }
    }
}