namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal interface IDataSource
    {
        IQualifiedMember SourceMember { get; }

        Expression SourceMemberTypeTest { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        bool IsFallback { get; }

        IList<ParameterExpression> Variables { get; }

        Expression Condition { get; }

        Expression Value { get; }

        Expression AddSourceCondition(Expression value);

        Expression FinalisePopulationBranch(Expression population, Expression alternatePopulation);
    }
}
