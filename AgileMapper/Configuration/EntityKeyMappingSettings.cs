namespace AgileObjects.AgileMapper.Configuration
{
    internal class EntityKeyMappingSettings : UserConfiguredItemBase
    {
        public static readonly EntityKeyMappingSettings MapAllKeys =
            new EntityKeyMappingSettings(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, mapKeys: true);

        public EntityKeyMappingSettings(MappingConfigInfo configInfo, bool mapKeys)
            : base(configInfo)
        {
            MapKeys = mapKeys;
        }

        public bool MapKeys { get; }
    }
}