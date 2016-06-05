namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class PreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> :
        InstanceCreationCallbackSpecifierBase<TSource, TTarget, TInstance>,
        IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>
    {
        public PreInstanceCreationCallbackSpecifier(MappingConfigInfo configInfo)
            : base(CallbackPosition.Before, configInfo)
        {
        }

        #region If Overloads

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private PreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> SetCondition(
            LambdaExpression conditionLambda)
        {
            ConfigInfo.AddCondition(conditionLambda);
            return this;
        }

        #endregion

        public void Call(Action<ITypedMemberMappingContext<TSource, TTarget>> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, int?> callback) => CreateCallbackFactory(callback);
    }
}