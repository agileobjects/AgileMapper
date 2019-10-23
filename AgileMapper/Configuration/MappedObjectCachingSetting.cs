namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
#endif
    using Members;

    internal class MappedObjectCachingSetting :
        UserConfiguredItemBase
#if NET35
        , IComparable<MappedObjectCachingSetting>
#endif
    {
        #region Singleton Instances

        public static readonly MappedObjectCachingSetting CacheAll =
            new MappedObjectCachingSetting(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, cache: true);

        public static readonly MappedObjectCachingSetting CacheNone =
            new MappedObjectCachingSetting(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, cache: false);

        #endregion

        public MappedObjectCachingSetting(MappingConfigInfo configInfo, bool cache)
            : base(configInfo)
        {
            Cache = cache;
        }

        public bool Cache { get; }

        public override bool ConflictsWith(UserConfiguredItemBase otherItem)
        {
            if ((otherItem == this) ||
               ((this == CacheNone) && (otherItem == CacheAll)) ||
               ((this == CacheAll) && (otherItem == CacheNone)))
            {
                return true;
            }

            if (base.ConflictsWith(otherItem))
            {
                var otherSettings = (MappedObjectCachingSetting)otherItem;

                if ((this == CacheNone) || (otherSettings == CacheNone) ||
                    (this == CacheAll) || (otherSettings == CacheAll))
                {
                    return (otherSettings.Cache == Cache);
                }

                // Settings have overlapping, non-global source and target types
                return true;
            }

            return false;
        }

        public string GetConflictMessage(MappedObjectCachingSetting conflicting)
        {
            if (conflicting == this)
            {
                return GetRedundantSettingConflictMessage(conflicting);
            }

            if ((this == CacheAll) && (conflicting == CacheNone))
            {
                return "Object tracking cannot be disabled globally with global identity integrity configured";
            }

            if ((this == CacheNone) && (conflicting == CacheAll))
            {
                return "Identity integrity cannot be configured globally with global object tracking disabled";
            }

            if (ConfigInfo.IsForAllSourceTypes())
            {
                return GetRedundantSettingConflictMessage(conflicting, " when mapping to " + TargetTypeName);
            }

            var typeSettings = $" when mapping {SourceTypeName} -> {TargetTypeName}";

            return GetRedundantSettingConflictMessage(conflicting, typeSettings);
        }

#if NET35
        int IComparable<MappedObjectCachingSetting>.CompareTo(MappedObjectCachingSetting other)
            => DoComparisonTo(other);
#endif
        private string GetRedundantSettingConflictMessage(
            MappedObjectCachingSetting conflicting,
            string typeSettings = null)
        {
            if (Cache == conflicting.Cache)
            {
                return Cache
                    ? "Identity integrity is already configured" + typeSettings
                    : "Object tracking is already disabled" + typeSettings;
            }

            return conflicting.Cache
                ? $"Identity integrity cannot be configured{typeSettings} with object tracking disabled"
                : $"Object tracking cannot be disabled{typeSettings} with identity integrity configured";
        }
    }
}