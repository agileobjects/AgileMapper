namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SourceAccessCounter : QuickUnwindExpressionVisitor
    {
        private readonly Expression _sourceValue;
        private int _numberOfAccesses;

        private SourceAccessCounter(Expression sourceValue)
        {
            _sourceValue = sourceValue;
        }

        public static bool MultipleAccessesExist(Expression sourceValue, Expression mapping)
        {
            var finder = new SourceAccessCounter(sourceValue);

            finder.Visit(mapping);

            return finder.HasMultipleAccesses;
        }

        protected override bool QuickUnwind => HasMultipleAccesses;

        private bool HasMultipleAccesses => _numberOfAccesses > 4;

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if (ExpressionEvaluation.AreEquivalent(memberAccess, _sourceValue))
            {
                ++_numberOfAccesses;
            }

            return base.VisitMember(memberAccess);
        }
    }
}