namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase
    {
        private readonly ConfiguredLambdaInfo _dataSourceLambda;

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
            _dataSourceLambda = dataSourceLambda;
        }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherDataSource = otherConfiguredItem as ConfiguredDataSourceFactory;

            if (otherDataSource == null)
            {
                return true;
            }

            if (SourceAndTargetTypesAreTheSame(otherDataSource))
            {
                return true;
            }

            return _dataSourceLambda.IsSameAs(otherDataSource._dataSourceLambda);
        }

        public IConfiguredDataSource Create(int dataSourceIndex, MemberMapperData data)
        {
            var configuredCondition = GetConditionOrNull(data);
            var value = _dataSourceLambda.GetBody(data);

            return new ConfiguredDataSource(dataSourceIndex, configuredCondition, value, data);
        }
    }
}