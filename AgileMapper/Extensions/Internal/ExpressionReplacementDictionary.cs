namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class ExpressionReplacementDictionary : Dictionary<Expression, Expression>
    {
        public ExpressionReplacementDictionary(int capacity)
            : base(capacity, ExpressionEquator.Instance)
        {
        }
    }
}