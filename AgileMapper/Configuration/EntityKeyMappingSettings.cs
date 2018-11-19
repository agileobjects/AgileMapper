namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
#endif

    internal class EntityKeyMappingSettings :
        UserConfiguredItemBase
#if NET35
        , IComparable<EntityKeyMappingSettings>
#endif
    {
        public static readonly EntityKeyMappingSettings MapAllKeys =
            new EntityKeyMappingSettings(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, mapKeys: true);

        public EntityKeyMappingSettings(MappingConfigInfo configInfo, bool mapKeys)
            : base(configInfo)
        {
            MapKeys = mapKeys;
        }

        public bool MapKeys { get; }

#if NET35
        int IComparable<EntityKeyMappingSettings>.CompareTo(EntityKeyMappingSettings other)
            => DoComparisonTo(other);
#endif
    }
}