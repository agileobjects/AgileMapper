namespace AgileObjects.AgileMapper.Configuration.MemberIgnores.SourceValueFilters
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static FilterConstants;

    internal class FilterOptimiser : ExpressionVisitor
    {
        private bool _incomplete;

        public static Expression Optimise(Expression expression)
        {
            if (expression == False)
            {
                return expression;
            }

            var optimiser = new FilterOptimiser();

            do
            {
                optimiser._incomplete = false;
                expression = optimiser.VisitAndConvert(expression, nameof(FilterOptimiser));
            }
            while (optimiser._incomplete);

            return expression;
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            switch (unary.NodeType)
            {
                case ExpressionType.Not when unary.Operand == False:
                    _incomplete = true;
                    return True;

                case ExpressionType.Not when unary.Operand == True:
                    _incomplete = true;
                    return False;

                default:
                    return base.VisitUnary(unary);
            }
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.AndAlso when binary.Left == False || binary.Right == False:
                    _incomplete = true;
                    return False;

                case ExpressionType.OrElse when binary.Left == True || binary.Right == True:
                    _incomplete = true;
                    return True;

                case ExpressionType.AndAlso when binary.Left == True:
                case ExpressionType.OrElse when binary.Left == False:
                    _incomplete = true;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return base.Visit(binary.Right);

                case ExpressionType.AndAlso when binary.Right == True:
                case ExpressionType.OrElse when binary.Right == False:
                    _incomplete = true;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return base.Visit(binary.Left);

                default:
                    return base.VisitBinary(binary);
            }
        }
    }
}