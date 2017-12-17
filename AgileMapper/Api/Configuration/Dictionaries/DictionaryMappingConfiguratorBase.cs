namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;

    internal abstract class DictionaryMappingConfiguratorBase<TSource, TTarget>
        : MappingConfigurator<TSource, TTarget>
    {
        protected DictionaryMappingConfiguratorBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        protected void SetupFlattenedTargetMemberNames(MappingConfigInfo configInfo = null)
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
        }

        protected void SetupMemberNameSeparator(string separator, MappingConfigInfo configInfo = null)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
        }

        protected void SetupElementKeyPattern(string pattern, MappingConfigInfo configInfo = null)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
        }
    }
}