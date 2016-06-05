namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> :
        InstanceCreationCallbackSpecifierBase<TSource, TTarget, TInstance>,
        IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>
    {
        public PostInstanceCreationCallbackSpecifier(MapperContext mapperContext)
            : this(new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        public PostInstanceCreationCallbackSpecifier(MappingConfigInfo configInfo)
            : base(CallbackPosition.After, configInfo)
        {
        }

        #region If Overloads

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, TInstance, int?, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<ITypedObjectMappingContext<TSource, TTarget, TInstance>, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> SetCondition(
            LambdaExpression conditionLambda)
        {
            ConfigInfo.AddCondition(conditionLambda);
            return this;
        }

        #endregion

        public void Call(Action<ITypedObjectMappingContext<TSource, TTarget, TInstance>> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, TInstance> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, TInstance, int?> callback) => CreateCallbackFactory(callback);
    }
}