namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Members;

    internal class ConfiguredDataSourceFactory :
        UserConfiguredItemBase,
        IComparable<ConfiguredDataSourceFactory>,
        IPotentialClone
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

            if (IsClone &&
               (otherConfiguredItem is IPotentialClone otherClone) &&
               !otherClone.IsClone)
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

        protected override bool MembersConflict(QualifiedMember otherMember)
            => TargetMember.LeafMember.Equals(otherMember.LeafMember);

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

        #endregion

        #region IComparable Members

        int IComparable<ConfiguredDataSourceFactory>.CompareTo(ConfiguredDataSourceFactory other)
        {
            var compareResult = ((IComparable<UserConfiguredItemBase>)this).CompareTo(other);

            if ((compareResult != 0) || (IsClone == other.IsClone))
            {
                return compareResult;
            }

            return IsClone ? 1 : -1;
        }

        #endregion
    }
}