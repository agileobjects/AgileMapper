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

    internal class ConfiguredSourceMemberFilterIgnore : ConfiguredSourceMemberIgnoreBase, IMemberFilterIgnore
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

        string IMemberFilterIgnore.MemberFilter => SourceMemberFilter;

        public override string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore)
            => ((IMemberFilterIgnore)this).GetConflictMessage(conflictingSourceMemberIgnore);

        public string GetConflictMessage(ConfiguredSourceMemberIgnore conflictingMemberIgnore)
            => ((IMemberFilterIgnore)this).GetConflictMessage(conflictingMemberIgnore);

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return base.AppliesTo(mapperData) &&
                  (mapperData.SourceMember is QualifiedMember sourceMember) &&
                   IsFiltered(sourceMember);
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is ConfiguredSourceMemberFilterIgnore otherIgnoredMemberFilter)
            {
                return SourceMemberFilter == otherIgnoredMemberFilter.SourceMemberFilter;
            }

            if (otherItem is ConfiguredSourceMemberIgnore otherIgnoredMember)
            {
                return IsFiltered(otherIgnoredMember.SourceMember);
            }

            return false;
        }

        public bool IsFiltered(QualifiedMember member)
            => _memberFilter.Invoke(new SourceMemberSelector(member));

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