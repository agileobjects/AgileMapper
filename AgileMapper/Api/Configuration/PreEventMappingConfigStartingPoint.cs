namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PreEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PreEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public SourceCallbackSpecifier<TSource, TTarget> CreatingTargetInstances
            => new SourceCallbackSpecifier<TSource, TTarget>(CallbackPosition.Before, _configInfo);
    }
}