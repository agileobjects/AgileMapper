namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
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
            Expression condition,
            ICollection<ParameterExpression> variables = null)
            : base(sourceMember, variables ?? Enumerable<ParameterExpression>.EmptyArray, value, condition)
        {
        }
    }
}