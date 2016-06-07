namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class PreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> :
        InstanceCreationCallbackSpecifierBase<TSource, TTarget, TObject>,
        IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
    {
        public PreInstanceCreationCallbackSpecifier(MappingConfigInfo configInfo)
            : base(CallbackPosition.Before, configInfo)
        {
        }

        #region If Overloads

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<ITypedObjectMappingContext<TSource, TTarget, TObject>, bool>> condition)
            => SetCondition(condition);

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private PreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> SetCondition(
            LambdaExpression conditionLambda)
        {
            ConfigInfo.AddCondition(conditionLambda);
            return this;
        }

        #endregion

        public void Call(Action<ITypedObjectMappingContext<TSource, TTarget, TObject>> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, int?> callback) => CreateCallbackFactory(callback);
    }
}