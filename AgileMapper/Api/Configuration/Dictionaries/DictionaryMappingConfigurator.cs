namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System.Linq.Expressions;
    using AgileMapper.Configuration;

    internal class DictionaryMappingConfigurator<TValue, TTarget> : IDictionaryMappingConfigurator<TValue, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public DictionaryMappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapKey(string key)
        {
            return new CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget>(
                _configInfo,
                Expression.Constant(key, typeof(string)));
        }
    }
}