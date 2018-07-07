namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IDataSource : IConditionallyChainable
    {
        IQualifiedMember SourceMember { get; }

        Expression SourceMemberTypeTest { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        ICollection<ParameterExpression> Variables { get; }

        Expression AddPreCondition(Expression population);

        Expression AddCondition(Expression value, Expression alternateBranch = null);
    }
}
