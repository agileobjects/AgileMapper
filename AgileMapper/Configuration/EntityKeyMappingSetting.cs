namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
#endif
    using Members;
    using ReadableExpressions.Extensions;

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
            if (otherItem == this)
            {
                return true;
            }

            if (base.ConflictsWith(otherItem))
            {
                var otherSettings = (EntityKeyMappingSetting)otherItem;

                if ((this == MapAllKeys) || (otherSettings == MapAllKeys))
                {
                    return (otherSettings.MapKeys == MapKeys);
                }

                // Settings have overlapping, non-global source and target types
                return true;
            }

            return false;
        }

        public string GetConflictMessage(EntityKeyMappingSetting conflicting)
        {
            var targetType = ConfigInfo.TargetType.GetFriendlyName();

            if (ConfigInfo.IsForAllSourceTypes())
            {
                return GetRedundantSettingConflictMessage(conflicting, " when mapping to " + targetType);
            }

            var sourceType = ConfigInfo.SourceType.GetFriendlyName();
            var typeSettings = $" when mapping {sourceType} -> {targetType}";

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