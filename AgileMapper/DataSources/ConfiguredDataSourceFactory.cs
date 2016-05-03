namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly Func<Expression, Expression> _customSourceValueFactory;

        private ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            Type targetType,
            Func<Expression, Expression> customSourceValueFactory,
            QualifiedMember targetMember)
            : base(configInfo, targetType, targetMember)
        {
            _customSourceValueFactory = customSourceValueFactory;
        }

        #region Factory Method

        public static ConfiguredDataSourceFactory For(
            MappingConfigInfo configInfo,
            Type targetType,
            Func<Expression, Expression> customSourceValueFactory,
            Expression targetMember)
        {
            return new ConfiguredDataSourceFactory(
                configInfo,
                targetType,
                customSourceValueFactory,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion

        public IDataSource Create(IConfigurationContext context)
        {
            var instance = ConfigInfo.IsForAllSources ? context.TargetVariable : context.SourceObject;
            var value = _customSourceValueFactory.Invoke(instance);

            return new ConfiguredDataSource(value, context.SourceObject, GetCondition);
        }
    }
}