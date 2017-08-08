namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ReadableExpressions;

    internal class ConfiguredIgnoredMember
        : UserConfiguredItemBase, IPotentialClone, IReverseConflictable
    {
        private readonly Expression _memberFilterLambda;
        private readonly Func<TargetMemberSelector, bool> _memberFilter;

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        public ConfiguredIgnoredMember(
            MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : base(configInfo, QualifiedMember.All)
        {
            _memberFilterLambda = memberFilterLambda.Body;
            _memberFilter = memberFilterLambda.Compile();
        }

        private ConfiguredIgnoredMember(
            MappingConfigInfo configInfo,
            QualifiedMember targetMember,
            Expression memberFilterLambda,
            Func<TargetMemberSelector, bool> memberFilter)
            : base(configInfo, targetMember)
        {
            _memberFilterLambda = memberFilterLambda;
            _memberFilter = memberFilter;
        }

        public string GetConflictMessage(UserConfiguredItemBase conflictingConfiguredItem)
        {
            if (conflictingConfiguredItem is ConfiguredDataSourceFactory conflictingDataSource)
            {
                return GetConflictMessage(conflictingDataSource);
            }

            return $"Member {TargetMember.GetPath()} has been ignored";
        }

        public string GetConflictMessage(ConfiguredIgnoredMember conflictingIgnoredMember)
        {
            var matcher = TargetMemberMatcher ?? conflictingIgnoredMember.TargetMemberMatcher;

            if (matcher == null)
            {
                return $"Member {TargetMember.GetPath()} has already been ignored";
            }

            if (TargetMemberMatcher == conflictingIgnoredMember.TargetMemberMatcher)
            {
                return $"Ignore pattern '{matcher}' has already been configured";
            }

            return $"Member {TargetMember.GetPath()} is already ignored by ignore pattern '{matcher}'";
        }

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            if (IsNotMemberFilterIngore)
            {
                return $"Ignored member {TargetMember.GetPath()} has a configured data source";
            }

            return $"Member ignore pattern '{TargetMemberMatcher}' conflicts with a configured data source";
        }

        public string GetIgnoreMessage(IQualifiedMember targetMember)
        {
            if (IsNotMemberFilterIngore)
            {
                return targetMember.Name + " is ignored";
            }

            return $"{targetMember.Name} is ignored by filter:{Environment.NewLine}{TargetMemberMatcher}";
        }

        private string TargetMemberMatcher => _memberFilterLambda?.ToReadableString();

        private bool IsNotMemberFilterIngore => _memberFilter == null;

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            return (IsNotMemberFilterIngore) ||
                    _memberFilter.Invoke(new TargetMemberSelector(mapperData.TargetMember));
        }

        protected override bool HasReverseConflict(UserConfiguredItemBase otherItem) => false;

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
        {
            if (IsNotMemberFilterIngore)
            {
                return base.MembersConflict(otherConfiguredItem);
            }

            if ((otherConfiguredItem is ConfiguredIgnoredMember otherIgnoredMember) &&
                (otherIgnoredMember.TargetMemberMatcher == TargetMemberMatcher))
            {
                return true;
            }

            return _memberFilter.Invoke(new TargetMemberSelector(otherConfiguredItem.TargetMember));
        }

        #region IPotentialClone Members

        public bool IsClone { get; private set; }

        public IPotentialClone Clone()
        {
            return new ConfiguredIgnoredMember(
                ConfigInfo,
                TargetMember,
                _memberFilterLambda,
                _memberFilter)
            {
                IsClone = true
            };
        }

        #endregion
    }
}