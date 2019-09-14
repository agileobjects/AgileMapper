namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal interface IDataSource : IConditionallyChainable
    {
        IQualifiedMember SourceMember { get; }

        Expression SourceMemberTypeTest { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        bool IsFallback { get; }

        IList<ParameterExpression> Variables { get; }

        Expression AddSourceCondition(Expression value);

        Expression FinalisePopulation(Expression population, Expression alternatePopulation = null);
    }
}
