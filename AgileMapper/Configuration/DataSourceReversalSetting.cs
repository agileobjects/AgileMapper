namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
#endif
    using Members;
    using ReadableExpressions.Extensions;

    internal class DataSourceReversalSetting :
        UserConfiguredItemBase
#if NET35
        , IComparable<DataSourceReversalSetting>
#endif
    {
        public static readonly DataSourceReversalSetting ReverseAll =
            new DataSourceReversalSetting(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes, reverse: true);

        public DataSourceReversalSetting(MappingConfigInfo configInfo, bool reverse)
            : base(configInfo)
        {
            Reverse = reverse;
        }

        public bool Reverse { get; }

        public override bool ConflictsWith(UserConfiguredItemBase otherItem)
        {
            if (otherItem == this)
            {
                return true;
            }

            if (base.ConflictsWith(otherItem))
            {
                var otherSettings = (DataSourceReversalSetting)otherItem;

                if ((this == ReverseAll) || (otherSettings == ReverseAll))
                {
                    return (otherSettings.Reverse == Reverse);
                }

                // Settings have overlapping, non-global source and target types
                return true;
            }

            return false;
        }

        public string GetConflictMessage(DataSourceReversalSetting conflicting)
        {
            if (ConfigInfo.IsForAllTargetTypes())
            {
                return GetRedundantSettingConflictMessage(conflicting, " by default");
            }

            // TODO: Test coverage?!
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
        int IComparable<DataSourceReversalSetting>.CompareTo(DataSourceReversalSetting other)
            => DoComparisonTo(other);
#endif
        private string GetRedundantSettingConflictMessage(
            DataSourceReversalSetting conflicting,
            string typeSettings)
        {
            if (Reverse == conflicting.Reverse)
            {
                return Reverse
                    ? "Configured data source reversal is already enabled" + typeSettings
                    : "Configured data source reversal is already disabled" + typeSettings;
            }

            return conflicting.Reverse
                ? $"Configured data source reversal cannot be enabled{typeSettings} as it has already been disabled"
                : $"Configured data source reversal cannot be disabled{typeSettings} as it has already been enabled";
        }
    }
}