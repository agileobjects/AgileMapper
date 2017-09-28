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
            string thisFilter = TargetMemberFilter, thatFilter = null;
            var matcher = thisFilter ?? (thatFilter = conflictingIgnoredMember.TargetMemberFilter);

            if (matcher == null)
            {
                return $"Member {TargetMember.GetPath()} has already been ignored";
            }

            if (thisFilter == (thatFilter ?? conflictingIgnoredMember.TargetMemberFilter))
            {
                return $"Ignore pattern '{matcher}' has already been configured";
            }

            return $"Member {TargetMember.GetPath()} is already ignored by ignore pattern '{matcher}'";
        }

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            if (HasMemberFilter)
            {
                return $"Member ignore pattern '{TargetMemberFilter}' conflicts with a configured data source";
            }

            return $"Ignored member {TargetMember.GetPath()} has a configured data source";
        }

        public string GetIgnoreMessage(IQualifiedMember targetMember)
        {
            if (HasMemberFilter)
            {
                return $"{targetMember.Name} is ignored by filter:{Environment.NewLine}{TargetMemberFilter}";
            }

            return targetMember.Name + " is ignored";
        }

        private bool HasMemberFilter => _memberFilter != null;

        private bool HasNoMemberFilter => !HasMemberFilter;

        private string TargetMemberFilter => _memberFilterLambda?.ToReadableString();

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            return HasNoMemberFilter ||
                  _memberFilter.Invoke(new TargetMemberSelector(mapperData.TargetMember));
        }

        protected override bool HasReverseConflict(UserConfiguredItemBase otherItem) => false;

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasNoMemberFilter)
            {
                return base.MembersConflict(otherConfiguredItem);
            }

            if ((otherConfiguredItem is ConfiguredIgnoredMember otherIgnoredMember) &&
                (otherIgnoredMember.TargetMemberFilter == TargetMemberFilter))
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

        public bool IsReplacementFor(IPotentialClone clonedIgnoredMember) => false;

        #endregion
    }
}