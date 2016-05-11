namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly Func<IMemberMappingContext, Expression> _customSourceValueFactory;

        private ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Func<IMemberMappingContext, Expression> customSourceValueFactory,
            QualifiedMember targetMember)
            : base(configInfo, mappingTargetType, targetMember)
        {
            _customSourceValueFactory = customSourceValueFactory;
        }

        #region Factory Method

        public static ConfiguredDataSourceFactory For(
            MappingConfigInfo configInfo,
            Type targetType,
            Func<IMemberMappingContext, Expression> customSourceValueFactory,
            Expression targetMember)
        {
            return new ConfiguredDataSourceFactory(
                configInfo,
                targetType,
                customSourceValueFactory,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion

        public IDataSource Create(IMemberMappingContext context)
        {
            var value = _customSourceValueFactory.Invoke(context);

            return new ConfiguredDataSource(value, context, GetCondition);
        }
    }
}