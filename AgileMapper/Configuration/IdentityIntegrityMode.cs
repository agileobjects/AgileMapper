namespace AgileObjects.AgileMapper.Configuration
{
    internal class IdentityIntegrityMode : UserConfiguredItemBase
    {
        public IdentityIntegrityMode(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        public static IdentityIntegrityMode MaintainAll(MapperContext mapperContext)
            => new IdentityIntegrityMode(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));
    }
}