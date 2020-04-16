namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class MappingContextValueInjector : IValueInjector
    {
        private readonly LambdaExpression _lambda;
        private readonly InvocationPosition _invocationPosition;
        private readonly ParameterExpression _contextParameter;
        private readonly Type _contextType;

        private MappingContextValueInjector(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition)
        {
            _lambda = lambda;
            _invocationPosition = invocationPosition.GetValueOrDefault();
            _contextParameter = lambda.Parameters[0];
            _contextType = _contextParameter.Type;
        }

        public static IValueInjector CreateFor(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition)
        {
            var requiredValues = MappingContextMemberAccessFinder.GetValuesRequiredBy(lambda);

            if (requiredValues.Values == default)
            {
                return new NullValueInjector(lambda);
            }

            if (requiredValues.Values.Has(LambdaValue.MappingContext))
            {
                return new MappingContextValueInjector(lambda, invocationPosition);
            }

            return ContextValuesValueInjector.Create(lambda, invocationPosition, requiredValues);
        }

        public Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
        {
            if (mapperData.MappingDataObject.Type.IsAssignableTo(_contextType))
            {
                return _lambda.ReplaceParameterWith(mapperData.MappingDataObject);
            }

            var args = new ValueInjectionArgs(_lambda, _invocationPosition, contextTypes, mapperData);
            var contextInfo = args.GetAppropriateMappingContext();

            if (_lambda.Body.NodeType == ExpressionType.Invoke)
            {
                return args.GetInvocationContextArgument(contextInfo);
            }

            //var targetContextTypes = _contextType.GetGenericTypeArguments();
            //var contextType = IsCallbackContext(targetContextTypes) ? _contextType : _contextType.GetAllInterfaces().First();

            //var sourceProperty = contextType.GetPublicInstanceProperty(RootSourceMemberName);
            //var targetProperty = contextType.GetPublicInstanceProperty(RootTargetMemberName);
            //var indexProperty = contextType.GetPublicInstanceProperty("ElementIndex");
            //var parentProperty = contextType.GetPublicInstanceProperty("Parent");

            //var replacementsByTarget = FixedSizeExpressionReplacementDictionary
            //    .WithEquivalentKeys(5)
            //    .Add(Property(_contextParameter, sourceProperty), contextInfo.SourceAccess)
            //    .Add(Property(_contextParameter, targetProperty), contextInfo.TargetAccess)
            //    .Add(Property(_contextParameter, indexProperty), contextInfo.Index)
            //    .Add(Property(_contextParameter, parentProperty), contextInfo.Parent);

            //if (IsObjectCreationContext(contextTypes))
            //{
            //    replacementsByTarget.Add(
            //        Property(_contextParameter, "CreatedObject"),
            //        contextInfo.CreatedObject);
            //}

            //return _lambda.Body.Replace(replacementsByTarget);
            return null;
        }
    }
}