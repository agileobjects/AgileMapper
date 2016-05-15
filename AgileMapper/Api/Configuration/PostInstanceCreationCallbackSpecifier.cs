namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public class PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>
        : InstanceCreationCallbackSpecifierBase<TSource, TTarget, TInstance>
    {
        internal PostInstanceCreationCallbackSpecifier(MapperContext mapperContext)
            : this(new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal PostInstanceCreationCallbackSpecifier(MappingConfigInfo configInfo)
            : base(CallbackPosition.After, configInfo)
        {
        }

        public PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<IInstanceCreationContext<TSource, TTarget, TInstance>> callback) => CreateCallback(callback);

        public PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<TSource, TTarget> callback) => CreateCallback(callback);

        public PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<TSource, TTarget, TInstance> callback) => CreateCallback(callback);

        public PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<TSource, TTarget, TInstance, int?> callback) => CreateCallback(callback);

        private PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> CreateCallback<TAction>(TAction callback)
        {
            var creationCallbackFactory = CreateCallbackFactory(callback);

            return new PostInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>(creationCallbackFactory);
        }
    }
}