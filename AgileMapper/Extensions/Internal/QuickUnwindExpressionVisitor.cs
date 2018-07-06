namespace AgileObjects.AgileMapper.Extensions.Internal
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal abstract class QuickUnwindExpressionVisitor : ExpressionVisitor
    {
        protected abstract bool QuickUnwind { get; }

        public override Expression Visit(Expression node)
            => QuickUnwind ? node : base.Visit(node);
    }
}
