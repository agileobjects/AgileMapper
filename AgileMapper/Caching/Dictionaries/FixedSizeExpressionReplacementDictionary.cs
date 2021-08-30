namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Extensions.Internal;

    internal class FixedSizeExpressionReplacementDictionary : FixedSizeSimpleDictionary<Expression, Expression>
    {
        private FixedSizeExpressionReplacementDictionary(int capacity, IEqualityComparer<Expression> expressionComparer)
            : base(capacity, expressionComparer)
        {
        }

        public static ISimpleDictionary<Expression, Expression> WithEqualKeys(int capacity)
            => new FixedSizeExpressionReplacementDictionary(capacity, ReferenceEqualsComparer<Expression>.Default);

        public static ISimpleDictionary<Expression, Expression> WithEquivalentKeys(int capacity)
            => new FixedSizeExpressionReplacementDictionary(capacity, ExpressionEvaluation.Equivalator);

        public override ISimpleDictionary<Expression, Expression> Add(Expression key, Expression value)
            => key != value ? base.Add(key, value) : this;
    }
}