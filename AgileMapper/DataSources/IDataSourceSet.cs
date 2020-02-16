namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IDataSourceSet
    {
        bool None { get; }

        bool HasValue { get; }

        bool IsConditional { get; }

        Expression SourceMemberTypeTest { get; }

        IList<ParameterExpression> Variables { get; }

        IDataSource this[int index] { get; }

        int Count { get; }

        Expression BuildValue();
    }
}