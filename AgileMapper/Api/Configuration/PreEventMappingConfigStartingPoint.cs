namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public class PreEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PreEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public ObjectCallbackSpecifier<TSource, TTarget, TSource> CreatingInstances
            => CreateCallbackSpecifier(creationTargetType: typeof(object));

        public ObjectCallbackSpecifier<TSource, TTarget, TSource> CreatingTargetInstances
            => CreateCallbackSpecifier(creationTargetType: typeof(TTarget));

        private ObjectCallbackSpecifier<TSource, TTarget, TSource> CreateCallbackSpecifier(Type creationTargetType)
            => new ObjectCallbackSpecifier<TSource, TTarget, TSource>(
                   CallbackPosition.Before,
                   _configInfo,
                   creationTargetType,
                   Callbacks.Source);
    }
}