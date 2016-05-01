namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        public ConfiguredIgnoredMember(
            MappingConfigInfo configInfo,
            Type targetType,
            QualifiedMember targetMember)
            : base(configInfo, targetType, targetMember)
        {
        }

        #region Factory Method

        public static ConfiguredIgnoredMember For(
            MappingConfigInfo configInfo,
            Type targetType,
            Expression targetMember)
        {
            return new ConfiguredIgnoredMember(
                configInfo,
                targetType,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion
    }
}