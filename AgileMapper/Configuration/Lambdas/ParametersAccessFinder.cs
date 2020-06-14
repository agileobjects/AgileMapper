namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ParametersAccessFinder : ExpressionVisitor
    {
        private readonly ParameterExpression _sourceParameter;
        private readonly ParameterExpression _targetParameter;
        private readonly ParameterExpression _createdObjectParameter;
        private readonly ParameterExpression _elementIndexParameter;
        private readonly RequiredValuesSet _requiredValues;

        private ParametersAccessFinder(IList<ParameterExpression> parameters)
        {
            _requiredValues = new RequiredValuesSet();

            var parameterCount = parameters.Count;
            _sourceParameter = parameters[0];

            if (parameterCount == 1)
            {
                return;
            }

            _targetParameter = parameters[1];

            if (parameterCount == 2)
            {
                return;
            }

            var thirdParameter = parameters[2];

            if (thirdParameter.Type != typeof(int?))
            {
                _createdObjectParameter = thirdParameter;
            }
            else
            {
                _elementIndexParameter = thirdParameter;
            }

            if (parameterCount == 3)
            {
                return;
            }

            _createdObjectParameter = thirdParameter;
            _elementIndexParameter = parameters[3];
        }

        public static RequiredValuesSet GetValuesRequiredBy(LambdaExpression lambda)
        {
            var finder = new ParametersAccessFinder(lambda.Parameters);
            finder.Visit(lambda.Body);

            return finder._requiredValues;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            if (parameter == _sourceParameter)
            {
                return _requiredValues.Source = parameter;
            }

            if (parameter == _targetParameter)
            {
                return _requiredValues.Target = parameter;
            }

            if (parameter == _createdObjectParameter)
            {
                return _requiredValues.CreatedObject = parameter;
            }

            if (parameter == _elementIndexParameter)
            {
                return _requiredValues.ElementIndex = parameter;
            }

            return base.VisitParameter(parameter);
        }
    }
}