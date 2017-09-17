namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;

    internal class DictionaryMappingConfigContinuation<TFirst, TSecond> :
        ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>,
        ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>
    {
        private readonly MappingConfigInfo _configInfo;

        public DictionaryMappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        ISourceDictionaryMappingConfigurator<TFirst, TSecond> ISourceDictionaryMappingConfigContinuation<TFirst, TSecond>.And
            => new SourceDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Clone());

        // TODO: Code coverage
        ITargetDictionaryMappingConfigurator<TFirst, TSecond> ITargetDictionaryMappingConfigContinuation<TFirst, TSecond>.And
            => new TargetDictionaryMappingConfigurator<TFirst, TSecond>(_configInfo.Clone());
    }
}