namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ReadableExpressions;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase, IReverseConflictable
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

        public string GetConflictMessage() => $"Member {TargetMember.GetPath()} has been ignored";

        public string GetConflictMessage(ConfiguredIgnoredMember conflictingIgnoredMember)
            => $"Member {TargetMember.GetPath()} has already been ignored";

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            if (_memberFilterLambda == null)
            {
                return $"Ignored member {TargetMember.GetPath()} has a configured data source";
            }

            var targetMemberMatcher = _memberFilterLambda.ToReadableString();

            return $"Member ignore pattern '{targetMemberMatcher}' conflicts with a configured data source";
        }

        public string GetIgnoreMessage(IQualifiedMember targetMember)
        {
            if (_memberFilterLambda == null)
            {
                return targetMember.Name + " is ignored";
            }

            var filter = _memberFilterLambda.ToReadableString();

            return $"{targetMember.Name} is ignored by filter:{Environment.NewLine}{filter}";
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            return (_memberFilter == null) ||
                    _memberFilter.Invoke(new TargetMemberSelector(mapperData.TargetMember));
        }

        protected override bool HasReverseConflict(UserConfiguredItemBase otherItem) => false;

        protected override bool MembersConflict(QualifiedMember otherMember)
        {
            if (_memberFilter == null)
            {
                return base.MembersConflict(otherMember);
            }

            return _memberFilter.Invoke(new TargetMemberSelector(otherMember));
        }
    }
}