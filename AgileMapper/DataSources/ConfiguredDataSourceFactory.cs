namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Api.Configuration;
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

        public IConfiguredDataSource Create(int dataSourceIndex, IMemberMappingContext context)
        {
            var value = _dataSourceLambda.GetBody(context);
            var condition = GetCondition(context);

            return new ConfiguredDataSource(dataSourceIndex, value, condition, context);
        }
    }
}