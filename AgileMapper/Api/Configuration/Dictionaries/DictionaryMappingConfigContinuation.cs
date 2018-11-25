namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
#if FEATURE_DYNAMIC
    using Dynamics;
#endif

    internal class DictionaryMappingConfigContinuation<TFirst, TSecond> :
        ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>,
        ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>
#if FEATURE_DYNAMIC
        ,
        ISourceDynamicMappingConfigContinuation<TSecond>,
        ITargetDynamicMappingConfigContinuation<TFirst>
#endif
    {
        private readonly MappingConfigInfo _configInfo;

        public DictionaryMappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        ISourceDictionaryMappingConfigurator<TFirst, TSecond> ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>.And
            => new SourceDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Copy());

        ITargetDictionaryMappingConfigurator<TFirst, TSecond> ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>.And
            => new TargetDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Copy());

#if FEATURE_DYNAMIC
        ISourceDynamicMappingConfigurator<TSecond> ISourceDynamicMappingConfigContinuation<TSecond>.And
            => new SourceDynamicMappingConfigurator<TSecond>(_configInfo.Copy());

        ITargetDynamicMappingConfigurator<TFirst> ITargetDynamicMappingConfigContinuation<TFirst>.And
            => new TargetDynamicMappingConfigurator<TFirst>(_configInfo.Copy());
#endif
    }
}