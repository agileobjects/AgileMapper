namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using Dynamics;

    internal class DictionaryMappingConfigContinuation<TFirst, TSecond> :
        ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>,
        ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>,
        ISourceDynamicMappingConfigContinuation<TSecond>
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

        ISourceDynamicMappingConfigurator<TSecond> ISourceDynamicMappingConfigContinuation<TSecond>.And
            => new SourceDynamicMappingConfigurator<TSecond>(_configInfo.Clone());
    }
}