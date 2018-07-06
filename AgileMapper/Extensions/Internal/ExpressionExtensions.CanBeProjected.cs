namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static partial class ExpressionExtensions
    {
        public static bool CanBeProjected(this LambdaExpression lambda)
            => ProjectableExpressionChecker.Check(lambda);

        private class ProjectableExpressionChecker : QuickUnwindExpressionVisitor
        {
            private readonly IList<ParameterExpression> _lambdaParameters;
            private bool _isNotProjectable;

            private ProjectableExpressionChecker(IList<ParameterExpression> lambdaParameters)
            {
                _lambdaParameters = lambdaParameters;
            }

            protected override bool QuickUnwind => _isNotProjectable;

            public static bool Check(LambdaExpression lambda)
            {
                var checker = new ProjectableExpressionChecker(lambda.Parameters);

                checker.Visit(lambda.Body);

                return !checker._isNotProjectable;
            }

            protected override Expression VisitInvocation(InvocationExpression invocation)
            {
                _isNotProjectable = true;

                return base.VisitInvocation(invocation);
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                if (IsNonSourceMappingDataMember(memberAccess))
                {
                    _isNotProjectable = true;
                }

                return base.VisitMember(memberAccess);
            }

            private static bool IsNonSourceMappingDataMember(MemberExpression memberAccess)
            {
                if (memberAccess.Member.DeclaringType.IsClosedTypeOf(typeof(IMappingData<,>)))
                {
                    return memberAccess.Member.Name != nameof(IMappingData<object, object>.Source);
                }

                return false;
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (_lambdaParameters.IndexOf(parameter) > 0)
                {
                    _isNotProjectable = true;
                }

                return base.VisitParameter(parameter);
            }
        }
    }
}
