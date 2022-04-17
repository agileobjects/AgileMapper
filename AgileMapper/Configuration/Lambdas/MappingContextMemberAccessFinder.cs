namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

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

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Object != _contextParameter)
            {
                return base.VisitMethodCall(methodCall);
            }

            if (methodCall.Method.DeclaringType == typeof(IServiceProviderAccessor))
            {
                _requiredValues.ServiceProvider = _contextParameter;
                return base.VisitMethodCall(methodCall);
            }

            _requiredValues.MappingContext = _contextParameter;
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
                    _requiredValues.MappingContext = memberAccess.Expression;
                    _requiredValues.Parent = memberAccess;
                    return memberAccess;

                case nameof(IMappingData<int, int>.Source):
                    return _requiredValues.Source = memberAccess;

                case nameof(IMappingData<int, int>.Target):
                    return _requiredValues.Target = memberAccess;

                case nameof(IMappingData<int, int>.ElementIndex):
                    return _requiredValues.ElementIndex = memberAccess;

                case nameof(IMappingData<int, int>.ElementKey):
                    return _requiredValues.ElementKey = memberAccess;
            }

            return base.VisitMember(memberAccess);
        }
    }
}