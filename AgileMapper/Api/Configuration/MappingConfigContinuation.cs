namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    public class MappingConfigContinuation<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal MappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IFullMappingConfigurator<TSource, TTarget> And
            => new MappingConfigurator<TSource, TTarget>(_configInfo.CloneForContinuation());
    }
}