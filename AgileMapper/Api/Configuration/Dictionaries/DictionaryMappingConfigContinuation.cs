namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
#if DYNAMIC_SUPPORTED
    using Dynamics;
#endif

    internal class DictionaryMappingConfigContinuation<TFirst, TSecond> :
        ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>,
        ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>
#if DYNAMIC_SUPPORTED
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
            => new SourceDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Clone());

        ITargetDictionaryMappingConfigurator<TFirst, TSecond> ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>.And
            => new TargetDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Clone());

#if DYNAMIC_SUPPORTED
        ISourceDynamicMappingConfigurator<TSecond> ISourceDynamicMappingConfigContinuation<TSecond>.And
            => new SourceDynamicMappingConfigurator<TSecond>(_configInfo.Clone());

        ITargetDynamicMappingConfigurator<TFirst> ITargetDynamicMappingConfigContinuation<TFirst>.And
            => new TargetDynamicMappingConfigurator<TFirst>(_configInfo.Clone());
#endif
    }
}