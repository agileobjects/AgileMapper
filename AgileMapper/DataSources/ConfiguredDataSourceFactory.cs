namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly ConfiguredLambdaInfo _dataSourceLambda;

        private ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            Type mappingTargetType,
            IQualifiedMember targetMember)
            : base(configInfo, mappingTargetType, targetMember)
        {
            _dataSourceLambda = dataSourceLambda;
        }

        #region Factory Method

        public static ConfiguredDataSourceFactory For(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            Type targetType,
            Expression targetMember)
        {
            return new ConfiguredDataSourceFactory(
                configInfo,
                dataSourceLambda,
                targetType,
                targetMember.ToTargetMember(configInfo.GlobalContext.MemberFinder));
        }

        #endregion

        public IDataSource Create(IMemberMappingContext context)
        {
            var value = _dataSourceLambda.GetBody(context);
            var condition = GetCondition(context);

            return new ConfiguredDataSource(value, context, condition);
        }
    }
}