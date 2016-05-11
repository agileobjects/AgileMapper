namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly LambdaExpression _dataSourceLambda;
        private readonly Func<LambdaExpression, IMemberMappingContext, Expression> _customSourceValueFactory;

        private ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            LambdaExpression dataSourceLambda,
            Type mappingTargetType,
            Func<LambdaExpression, IMemberMappingContext, Expression> customSourceValueFactory,
            QualifiedMember targetMember)
            : base(configInfo, mappingTargetType, targetMember)
        {
            _dataSourceLambda = dataSourceLambda;
            _customSourceValueFactory = customSourceValueFactory;
        }

        #region Factory Method

        public static ConfiguredDataSourceFactory For(
            MappingConfigInfo configInfo,
            LambdaExpression dataSourceLambda,
            Type targetType,
            Func<LambdaExpression, IMemberMappingContext, Expression> customSourceValueFactory,
            Expression targetMember)
        {
            return new ConfiguredDataSourceFactory(
                configInfo,
                dataSourceLambda,
                targetType,
                customSourceValueFactory,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion

        public IDataSource Create(IMemberMappingContext context)
        {
            var value = _customSourceValueFactory.Invoke(_dataSourceLambda, context);

            return new ConfiguredDataSource(value, context, GetCondition);
        }
    }
}