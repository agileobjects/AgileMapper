namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Api.Configuration;
    using Members;

    public class PreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>
        : InstanceCreationCallbackSpecifierBase<TSource, TTarget, TInstance>
    {
        internal PreInstanceCreationCallbackSpecifier(MappingConfigInfo configInfo)
            : base(CallbackPosition.Before, configInfo)
        {
        }

        public PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<ITypedMemberMappingContext<TSource, TTarget>> callback) => CreateCallback(callback);

        public PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<TSource, TTarget, int?> callback) => CreateCallback(callback);

        private PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance> CreateCallback<TAction>(TAction callback)
        {
            var creationCallbackFactory = CreateCallbackFactory(callback);

            return new PreInstanceCreationConditionSpecifier<TSource, TTarget, TInstance>(creationCallbackFactory);
        }
    }
}