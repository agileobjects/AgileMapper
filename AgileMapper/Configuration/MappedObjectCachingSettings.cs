namespace AgileObjects.AgileMapper.Configuration
{
    using ReadableExpressions.Extensions;

    internal class MappedObjectCachingSettings : UserConfiguredItemBase
    {
        #region Singleton Instances

        public static readonly MappedObjectCachingSettings CacheAll =
            new MappedObjectCachingSettings(ForAllMappings(MapperContext.Default), cache: true);

        public static readonly MappedObjectCachingSettings CacheNone =
            new MappedObjectCachingSettings(ForAllMappings(MapperContext.Default), cache: false);

        private static MappingConfigInfo ForAllMappings(MapperContext mapperContext)
            => MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext);

        #endregion

        public MappedObjectCachingSettings(MappingConfigInfo configInfo, bool cache)
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
                var otherSettings = (MappedObjectCachingSettings)otherItem;

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

        public string GetConflictMessage(MappedObjectCachingSettings conflicting)
        {
            if (conflicting == this)
            {
                return GetRedundantSettingsConflictMessage(conflicting);
            }

            if ((this == CacheAll) && (conflicting == CacheNone))
            {
                return "Object tracking cannot be disabled globally with global identity integrity configured";
            }

            if ((this == CacheNone) && (conflicting == CacheAll))
            {
                return "Identity integrity cannot be configured globally with global object tracking disabled";
            }

            var sourceType = ConfigInfo.SourceType.GetFriendlyName();
            var targetType = ConfigInfo.TargetType.GetFriendlyName();
            var typeSettings = $" when mapping {sourceType} -> {targetType}";

            return GetRedundantSettingsConflictMessage(conflicting, typeSettings);
        }

        private string GetRedundantSettingsConflictMessage(
            MappedObjectCachingSettings conflicting,
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