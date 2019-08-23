namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
    using Extensions.Internal;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using ReadableExpressions;

    internal class ConfiguredSourceMemberFilterIgnore : ConfiguredSourceMemberIgnoreBase
    {
        private readonly Expression _memberFilterExpression;
        private readonly Func<SourceMemberSelector, bool> _memberFilter;
#if NET35
        public ConfiguredSourceMemberFilterIgnore(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<SourceMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredSourceMemberFilterIgnore(
            MappingConfigInfo configInfo,
            Expression<Func<SourceMemberSelector, bool>> memberFilterLambda)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterLambda.Body;
            _memberFilter = memberFilterLambda.Compile();
        }

        private ConfiguredSourceMemberFilterIgnore(
            MappingConfigInfo configInfo,
            Expression memberFilterExpression,
            Func<SourceMemberSelector, bool> memberFilter)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterExpression;
            _memberFilter = memberFilter;
        }

        private string SourceMemberFilter => _memberFilterExpression?.ToReadableString();

        public override string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore)
        {
            if (conflictingSourceMemberIgnore is ConfiguredSourceMemberIgnore otherIgnoredMember)
            {
                return GetConflictMessage(otherIgnoredMember);
            }

            var otherIgnoredMemberFilter = (ConfiguredSourceMemberFilterIgnore)conflictingSourceMemberIgnore;

            return $"Ignore pattern '{otherIgnoredMemberFilter.SourceMemberFilter}' has already been configured";
        }

        public string GetConflictMessage(ConfiguredSourceMemberIgnore otherMemberIgnore)
        {
            return $"Member {otherMemberIgnore.SourceMember.GetPath()} is " +
                   $"already ignored by ignore pattern '{SourceMemberFilter}'";
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return base.AppliesTo(mapperData) &&
                   (mapperData.SourceMember is QualifiedMember sourceMember) &&
                   _memberFilter.Invoke(new SourceMemberSelector(sourceMember));
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is ConfiguredSourceMemberFilterIgnore otherIgnoredMemberFilter)
            {
                return SourceMemberFilter == otherIgnoredMemberFilter.SourceMemberFilter;
            }

            if (otherItem is ConfiguredSourceMemberIgnore otherIgnoredMember)
            {
                return _memberFilter.Invoke(new SourceMemberSelector(otherIgnoredMember.SourceMember));
            }

            return false;
        }

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredSourceMemberFilterIgnore(
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