namespace AgileObjects.AgileMapper.Configuration
{
    internal class MappedObjectCachingSettings : UserConfiguredItemBase
    {
        public MappedObjectCachingSettings(MappingConfigInfo configInfo, bool cache)
            : base(configInfo)
        {
            Cache = cache;
        }

        #region Factory Methods

        public static MappedObjectCachingSettings CacheAll(MapperContext mapperContext)
            => new MappedObjectCachingSettings(ForAllMappings(mapperContext), cache: true);

        public static MappedObjectCachingSettings CacheNone(MapperContext mapperContext)
            => new MappedObjectCachingSettings(ForAllMappings(mapperContext), cache: false);

        private static MappingConfigInfo ForAllMappings(MapperContext mapperContext)
            => MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext);

        #endregion

        public bool Cache { get; }
    }
}