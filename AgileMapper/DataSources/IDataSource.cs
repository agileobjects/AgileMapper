namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal interface IDataSource : IConditionallyChainable
    {
        IQualifiedMember SourceMember { get; }

        Expression SourceMemberTypeTest { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        IEnumerable<ParameterExpression> Variables { get; }
    }
}
