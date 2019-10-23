namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
#endif
    using Members;

    internal class EntityKeyMappingSetting :
        UserConfiguredItemBase
#if NET35
        , IComparable<EntityKeyMappingSetting>
#endif
    {
        public static readonly EntityKeyMappingSetting MapAllKeys =
            new EntityKeyMappingSetting(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, mapKeys: true);

        public EntityKeyMappingSetting(MappingConfigInfo configInfo, bool mapKeys)
            : base(configInfo)
        {
            MapKeys = mapKeys;
        }

        public bool MapKeys { get; }

        public override bool ConflictsWith(UserConfiguredItemBase otherItem)
        {
            if (!base.ConflictsWith(otherItem))
            {
                return false;
            }

            var otherSettings = (EntityKeyMappingSetting)otherItem;

            if ((this == MapAllKeys) || (otherSettings == MapAllKeys))
            {
                return (otherSettings.MapKeys == MapKeys);
            }

            // Settings have overlapping, non-global source and target types
            return true;

        }

        public string GetConflictMessage(EntityKeyMappingSetting conflicting)
        {
            if (ConfigInfo.IsForAllSourceTypes())
            {
                return GetRedundantSettingConflictMessage(conflicting, " when mapping to " + TargetTypeName);
            }

            var typeSettings = $" when mapping {SourceTypeName} -> {TargetTypeName}";

            return GetRedundantSettingConflictMessage(conflicting, typeSettings);
        }

#if NET35
        int IComparable<EntityKeyMappingSetting>.CompareTo(EntityKeyMappingSetting other)
            => DoComparisonTo(other);
#endif
        private string GetRedundantSettingConflictMessage(
            EntityKeyMappingSetting conflicting,
            string typeSettings)
        {
            if (MapKeys == conflicting.MapKeys)
            {
                return MapKeys
                    ? "Entity key mapping is already enabled" + typeSettings
                    : "Entity key mapping is already disabled" + typeSettings;
            }

            return conflicting.MapKeys
                ? $"Entity key mapping cannot be enabled{typeSettings} as it has already been disabled"
                : $"Entity key mapping cannot be disabled{typeSettings} as it has already been enabled";
        }
    }
}