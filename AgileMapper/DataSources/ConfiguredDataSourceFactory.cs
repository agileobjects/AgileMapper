namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory : UserConfiguredItemBase, IComparable<ConfiguredDataSourceFactory>
    {
        private readonly ConfiguredLambdaInfo _dataSourceLambda;
        private bool _isClone;

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

            if (_isClone && !otherDataSource._isClone)
            {
                return false;
            }

            if (SourceAndTargetTypesAreTheSame(otherDataSource))
            {
                return true;
            }

            return _dataSourceLambda.IsSameAs(otherDataSource._dataSourceLambda);
        }

        protected override bool MembersConflict(QualifiedMember otherMember)
            => TargetMember.LeafMember.Equals(otherMember.LeafMember);

        public IConfiguredDataSource Create(IMemberMapperData mapperData)
        {
            var configuredCondition = GetConditionOrNull(mapperData);
            var value = _dataSourceLambda.GetBody(mapperData);

            return new ConfiguredDataSource(configuredCondition, value, mapperData);
        }

        public ConfiguredDataSourceFactory Clone()
        {
            return new ConfiguredDataSourceFactory(ConfigInfo, _dataSourceLambda, TargetMember)
            {
                _isClone = true
            };
        }

        int IComparable<ConfiguredDataSourceFactory>.CompareTo(ConfiguredDataSourceFactory other)
        {
            var compareResult = ((IComparable<UserConfiguredItemBase>)this).CompareTo(other);

            if ((compareResult != 0) || (_isClone == other._isClone))
            {
                return compareResult;
            }

            return _isClone ? 1 : -1;
        }
    }
}