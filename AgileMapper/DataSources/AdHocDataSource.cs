namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class AdHocDataSource : DataSourceBase
    {
        public AdHocDataSource(IQualifiedMember sourceMember, Expression value)
            : base(sourceMember, value)
        {
        }

        public AdHocDataSource(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMapperData mapperData)
            : base(sourceMember, value, mapperData)
        {
        }

        public AdHocDataSource(
            IQualifiedMember sourceMember,
            Expression value,
            Expression condition)
            : this(
                sourceMember,
                value,
                condition,
                Enumerable<ParameterExpression>.EmptyArray)
        {
        }

        public AdHocDataSource(
            IQualifiedMember sourceMember,
            Expression value,
            Expression condition,
            IList<ParameterExpression> variables)
            : base(
                sourceMember,
                variables,
                value,
                condition)
        {
        }
    }
}