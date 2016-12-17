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

        Expression AddPreCondition(Expression population);

        Expression AddCondition(Expression value, Expression alternateBranch = null);

        Expression GetMemberPopulation(IMemberMapperData mapperData);
    }
}
