namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Linq.Expressions;

    internal abstract class QuickUnwindExpressionVisitor : ExpressionVisitor
    {
        protected abstract bool QuickUnwind { get; }

        public override Expression Visit(Expression node)
            => QuickUnwind ? node : base.Visit(node);
    }
}
