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

        protected void SetupFlattenedTargetMemberNames()
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
        }

        protected void SetupMemberNameSeparator(string separator)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
        }

        protected void SetupElementKeyPattern(string pattern)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
        }
    }
}