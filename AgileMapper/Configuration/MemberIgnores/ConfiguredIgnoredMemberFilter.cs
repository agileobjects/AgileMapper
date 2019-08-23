namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using DataSources.Factories;
#if NET35
    using Extensions.Internal;
#endif
    using Members;
    using ReadableExpressions;

    internal class ConfiguredIgnoredMemberFilter : ConfiguredIgnoredMemberBase
    {
        private readonly Expression _memberFilterExpression;
        private readonly Func<TargetMemberSelector, bool> _memberFilter;
#if NET35
        public ConfiguredIgnoredMemberFilter(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredIgnoredMemberFilter(
            MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.Body, memberFilterLambda.Compile())
        {
        }

        private ConfiguredIgnoredMemberFilter(
            MappingConfigInfo configInfo,
            Expression memberFilterExpression,
            Func<TargetMemberSelector, bool> memberFilter)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterExpression;
            _memberFilter = memberFilter;
        }

        private string TargetMemberFilter => _memberFilterExpression?.ToReadableString();

        public override string GetConflictMessage(ConfiguredIgnoredMemberBase conflictingIgnoredMember)
        {
            if (conflictingIgnoredMember is ConfiguredIgnoredMember otherIgnoredMember)
            {
                return GetConflictMessage(otherIgnoredMember);
            }

            var otherIgnoredMemberFilter = (ConfiguredIgnoredMemberFilter)conflictingIgnoredMember;

            return $"Ignore pattern '{otherIgnoredMemberFilter.TargetMemberFilter}' has already been configured";
        }

        public string GetConflictMessage(ConfiguredIgnoredMember otherIgnoredMember)
        {
            return $"Member {otherIgnoredMember.TargetMember.GetPath()} is " +
                   $"already ignored by ignore pattern '{TargetMemberFilter}'";
        }

        public override string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
            => $"Member ignore pattern '{TargetMemberFilter}' conflicts with a configured data source";

        public override string GetIgnoreMessage(IQualifiedMember targetMember)
            => $"{targetMember.Name} is ignored by filter:{Environment.NewLine}{TargetMemberFilter}";

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return base.AppliesTo(mapperData) &&
                   _memberFilter.Invoke(new TargetMemberSelector(mapperData.TargetMember));
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is ConfiguredIgnoredMemberFilter otherIgnoredMemberFilter)
            {
                return otherIgnoredMemberFilter.TargetMemberFilter == TargetMemberFilter;
            }

            return _memberFilter.Invoke(new TargetMemberSelector(otherItem.TargetMember));
        }

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredIgnoredMemberFilter(
                ConfigInfo,
                _memberFilterExpression,
                _memberFilter)
            {
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem) => false;

        #endregion
    }
}