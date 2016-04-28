namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredDataSource : UserConfiguredItemBase, IDataSource
    {
        private readonly Func<Expression, Expression> _customSourceValueFactory;

        private ConfiguredDataSource(
            MappingConfigInfo configInfo,
            Type targetType,
            Func<Expression, Expression> customSourceValueFactory,
            QualifiedMember targetMember)
            : base(configInfo, targetType, targetMember)
        {
            _customSourceValueFactory = customSourceValueFactory;
        }

        #region Factory Method

        public static ConfiguredDataSource For(
            MappingConfigInfo configInfo,
            Type targetType,
            Func<Expression, Expression> customSourceValueFactory,
            Expression targetMember)
        {
            return new ConfiguredDataSource(
                configInfo,
                targetType,
                customSourceValueFactory,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion

        public bool IsUserConfigured => true;

        public Expression GetValue(IObjectMappingContext omc)
        {
            var instance = ConfigInfo.IsForAllSources ? omc.TargetVariable : omc.SourceObject;
            var value = _customSourceValueFactory.Invoke(instance);

            return value;
        }
    }
}