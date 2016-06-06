namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly ConfiguredLambdaInfo _dataSourceLambda;

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            Type mappingTargetType,
            LambdaExpression targetMemberLambda)
            : base(configInfo, mappingTargetType, targetMemberLambda)
        {
            _dataSourceLambda = dataSourceLambda;
        }

        public IConfiguredDataSource Create(int dataSourceIndex, IMemberMappingContext context)
        {
            var value = _dataSourceLambda.GetBody(context);
            var condition = GetCondition(context);

            return new ConfiguredDataSource(dataSourceIndex, value, condition, context);
        }
    }
}