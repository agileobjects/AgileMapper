namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IDataSource
    {
        IQualifiedMember SourceMember { get; }

        Expression SourceMemberTypeTest { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        Expression Condition { get; }

        IEnumerable<ParameterExpression> Variables { get; }

        Expression Value { get; }
    }
}
