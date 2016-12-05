namespace AgileObjects.AgileMapper.Configuration
{
    internal class NullCollectionsSetting : UserConfiguredItemBase
    {
        public NullCollectionsSetting(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        public static NullCollectionsSetting AlwaysMapToNull(MapperContext mapperContext)
            => new NullCollectionsSetting(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));
    }
}