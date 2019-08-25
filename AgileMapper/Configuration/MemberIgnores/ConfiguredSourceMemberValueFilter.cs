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
    using Api.Configuration;

    internal class ConfiguredSourceMemberValueFilter : ConfiguredSourceMemberIgnoreBase
    {
        private readonly Expression _valuesFilterExpression;
#if NET35
        public ConfiguredSourceMemberValueFilter(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<SourceValueIgnoreSpecifier, bool>> valuesFilter)
            : this(configInfo, valuesFilter.ToDlrExpression())
        {
        }
#endif
        public ConfiguredSourceMemberValueFilter(
            MappingConfigInfo configInfo,
            Expression<Func<SourceValueIgnoreSpecifier, bool>> valuesFilter)
            : base(configInfo)
        {
            _valuesFilterExpression = valuesFilter.Body;
        }

        public override string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore)
        {
            throw new NotImplementedException();
        }

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            throw new NotImplementedException();
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}