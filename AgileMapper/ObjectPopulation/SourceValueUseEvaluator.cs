namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SourceValueUseEvaluator : QuickUnwindExpressionVisitor
    {
        private readonly Expression _sourceValue;
        private int _numberOfAccesses;

        private SourceValueUseEvaluator(Expression sourceValue)
        {
            _sourceValue = sourceValue;
        }

        public static bool UseLocalVariableFor(Expression sourceValue, Expression mapping)
        {
            var finder = new SourceValueUseEvaluator(sourceValue);
            finder.Visit(mapping);

            return !finder.IsAlreadyAssigned && finder.HasMultipleAccesses;
        }

        protected override bool QuickUnwind => IsAlreadyAssigned || HasMultipleAccesses;

        private bool IsAlreadyAssigned { get; set; }

        private bool HasMultipleAccesses => _numberOfAccesses > 4;

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (binary.NodeType == ExpressionType.Assign &&
                binary.Left.NodeType == ExpressionType.Parameter &&
                MatchesSourceValue(binary.Right))
            {
                IsAlreadyAssigned = true;
            }

            return base.VisitBinary(binary);
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if (MatchesSourceValue(memberAccess))
            {
                ++_numberOfAccesses;
            }

            return base.VisitMember(memberAccess);
        }

        private bool MatchesSourceValue(Expression expression)
            => ExpressionEvaluation.AreEquivalent(expression, _sourceValue);
    }
}