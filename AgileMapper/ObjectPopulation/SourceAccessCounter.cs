namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions.Internal;

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
            if (memberAccess == _sourceValue)
            {
                ++_numberOfAccesses;
            }

            return base.VisitMember(memberAccess);
        }
    }
}