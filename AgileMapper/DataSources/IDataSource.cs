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

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDataSource"/> is to be applied to the
        /// mapping target object sequentially, <em>i.e</em> before or after other data sources are
        /// applied.
        /// </summary>
        bool IsSequential { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDataSource"/> provides a fallback value
        /// to be applied to a mapping target when other data sources cannot be found or do not apply.
        /// </summary>
        bool IsFallback { get; }

        IList<ParameterExpression> Variables { get; }

        Expression Condition { get; }

        Expression Value { get; }

        Expression AddSourceCondition(Expression value);

        Expression FinalisePopulationBranch(
            Expression alternatePopulation, 
            IDataSource nextDataSource,
            IMemberMapperData mapperData);
    }
}
