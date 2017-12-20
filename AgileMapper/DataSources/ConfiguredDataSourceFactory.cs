namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase, IPotentialClone
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
            var dataSourceLambdasAreTheSame = HasSameDataSourceLambdaAs(otherDataSource);

            if (IsClone &&
               (otherConfiguredItem is IPotentialClone otherClone) &&
               !otherClone.IsClone)
            {
                return (otherDataSource != null) && dataSourceLambdasAreTheSame;
            }

            if (otherDataSource == null)
            {
                return true;
            }

            if (SourceAndTargetTypesAreTheSame(otherDataSource))
            {
                return true;
            }

            return dataSourceLambdasAreTheSame;
        }

        private bool HasSameDataSourceLambdaAs(ConfiguredDataSourceFactory otherDataSource)
        {
            return _dataSourceLambda.IsSameAs(otherDataSource?._dataSourceLambda);
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
            => TargetMember.LeafMember.Equals(otherConfiguredItem.TargetMember.LeafMember);

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

        #region IPotentialClone Members

        public bool IsClone { get; private set; }

        public IPotentialClone Clone()
        {
            return new ConfiguredDataSourceFactory(ConfigInfo, _dataSourceLambda, TargetMember)
            {
                IsClone = true
            };
        }

        public bool IsReplacementFor(IPotentialClone clonedDataSourceFactory)
            => ConflictsWith((ConfiguredDataSourceFactory)clonedDataSourceFactory);

        #endregion
    }
}