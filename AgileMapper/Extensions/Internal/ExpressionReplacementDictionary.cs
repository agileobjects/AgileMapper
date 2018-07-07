namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ExpressionReplacementDictionary : Dictionary<Expression, Expression>
    {
        public ExpressionReplacementDictionary(int capacity)
            : base(capacity, ExpressionEvaluation.Equivalator)
        {
        }
    }
}