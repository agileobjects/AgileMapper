namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using NetStandardPolyfills;

    internal static partial class ExpressionExtensions
    {
        public static bool CanBeProjected(this LambdaExpression lambda)
            => ProjectableExpressionChecker.Check(lambda);

        private class ProjectableExpressionChecker : ExpressionVisitor
        {
            private readonly IList<ParameterExpression> _lambdaParameters;
            private bool _canBeProjected;

            private ProjectableExpressionChecker(IList<ParameterExpression> lambdaParameters)
            {
                _lambdaParameters = lambdaParameters;
                _canBeProjected = true;
            }

            public static bool Check(LambdaExpression lambda)
            {
                var checker = new ProjectableExpressionChecker(lambda.Parameters);

                checker.Visit(lambda.Body);

                return checker._canBeProjected;
            }

            protected override Expression VisitInvocation(InvocationExpression invocation)
            {
                _canBeProjected = false;

                return base.VisitInvocation(invocation);
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                if (IsNonSourceMappingDataMember(memberAccess))
                {
                    _canBeProjected = false;
                }

                return base.VisitMember(memberAccess);
            }

            private static bool IsNonSourceMappingDataMember(MemberExpression memberAccess)
            {
                if (memberAccess.Member.DeclaringType.IsClosedTypeOf(typeof(IMappingData<,>)))
                {
                    return memberAccess.Member.Name == nameof(IMappingData<object, object>.Source);
                }

                return false;
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (_lambdaParameters.IndexOf(parameter) > 0)
                {
                    _canBeProjected = false;
                }

                return base.VisitParameter(parameter);
            }
        }
    }
}
