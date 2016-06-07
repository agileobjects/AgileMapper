namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class PostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> :
        InstanceCreationCallbackSpecifierBase<TSource, TTarget, TObject>,
        IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
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

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<ITypedObjectCreationMappingContext<TSource, TTarget, TObject>, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        public IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, TObject, int?, bool>> condition)
            => SetCondition(condition);

        private PostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> SetCondition(
            LambdaExpression conditionLambda)
        {
            ConfigInfo.AddCondition(conditionLambda);
            return this;
        }

        #endregion

        public void Call(Action<ITypedObjectCreationMappingContext<TSource, TTarget, TObject>> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, TObject> callback) => CreateCallbackFactory(callback);

        public void Call(Action<TSource, TTarget, TObject, int?> callback) => CreateCallbackFactory(callback);
    }
}