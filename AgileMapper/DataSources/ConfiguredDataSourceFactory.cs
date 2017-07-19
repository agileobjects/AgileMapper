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
            QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
            _dataSourceLambda = dataSourceLambda;
        }

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

            return HasSameDataSourceLambdaAs(otherDataSource);
        }

        private bool HasSameDataSourceLambdaAs(ConfiguredDataSourceFactory otherDataSource)
        {
            return _dataSourceLambda.IsSameAs(otherDataSource?._dataSourceLambda);
        }

        protected override bool MembersConflict(QualifiedMember otherMember)
            => TargetMember.LeafMember.Equals(otherMember.LeafMember);

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            var lambdasAreTheSame = HasSameDataSourceLambdaAs(conflictingDataSource);
            var conflictIdentifier = lambdasAreTheSame ? "that" : "a";

            return $"{TargetMember.GetPath()} already has {conflictIdentifier} configured data source";
        }

        public IConfiguredDataSource Create(IMemberMapperData mapperData)
        {
            var configuredCondition = GetConditionOrNull(mapperData);
            var value = _dataSourceLambda.GetBody(mapperData);

            return new ConfiguredDataSource(configuredCondition, value, mapperData);
        }
    }
}