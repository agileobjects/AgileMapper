namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using static Members.Member;

    internal class MappingContextMemberAccessFinder : ExpressionVisitor
    {
        private readonly ParameterExpression _contextParameter;
        private readonly RequiredValuesSet _requiredValues;

        private MappingContextMemberAccessFinder(ParameterExpression contextParameter)
        {
            _contextParameter = contextParameter;
            _requiredValues = new RequiredValuesSet();
        }

        public static RequiredValuesSet GetValuesRequiredBy(LambdaExpression lambda)
        {
            var finder = new MappingContextMemberAccessFinder(lambda.Parameters[0]);
            finder.Visit(lambda.Body);

            return finder._requiredValues;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            if (parameter == _contextParameter)
            {
                _requiredValues.MappingContext = parameter;
            }

            return base.VisitParameter(parameter);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Object == _contextParameter)
            {
                _requiredValues.MappingContext = _contextParameter;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if (memberAccess.Expression != _contextParameter)
            {
                return base.VisitMember(memberAccess);
            }

            switch (memberAccess.Member.Name)
            {
                case nameof(IMappingData.Parent):
                    _requiredValues.Parent = memberAccess;
                    break;

                case RootSourceMemberName:
                    _requiredValues.Source = memberAccess;
                    break;

                case RootTargetMemberName:
                    _requiredValues.Target = memberAccess;
                    break;

                case nameof(IMappingData<int, int>.ElementIndex):
                    _requiredValues.ElementIndex = memberAccess;
                    break;

                case nameof(IMappingData<int, int>.ElementKey):
                    _requiredValues.ElementKey = memberAccess;
                    break;
            }

            return base.VisitMember(memberAccess);
        }
    }
}