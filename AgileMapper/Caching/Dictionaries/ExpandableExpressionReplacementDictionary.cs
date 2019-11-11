namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ExpandableExpressionReplacementDictionary : ExpandableSimpleDictionary<Expression, Expression>
    {
        public ExpandableExpressionReplacementDictionary()
            : base(10, ReferenceEqualsComparer<Expression>.Default)
        {
        }
    }
}