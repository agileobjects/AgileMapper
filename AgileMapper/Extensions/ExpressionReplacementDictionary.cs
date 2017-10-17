namespace AgileObjects.AgileMapper.Extensions
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