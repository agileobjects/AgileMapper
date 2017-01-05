namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;

    internal abstract class DictionaryMappingConfiguratorBase<TSource, TTarget>
        : MappingConfigurator<TSource, TTarget>
    {
        protected DictionaryMappingConfiguratorBase(MappingConfigInfo configInfo)
            : base(configInfo.ForTargetType<TTarget>())
        {
        }

        protected void SetupFlattenedMemberNames()
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
        }
    }
}