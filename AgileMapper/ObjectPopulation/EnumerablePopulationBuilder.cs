namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class EnumerablePopulationBuilder
    {
        #region Cached Items

        private static readonly MethodInfo _selectMethod = typeof(Enumerable)
            .GetMethods(Constants.PublicStatic)
            .Last(m => (m.Name == "Select") && m.GetParameters().Length == 2);

        private static readonly MethodInfo _intersectByIdMethod = typeof(EnumerableExtensions)
            .GetMethod("IntersectById", Constants.PublicStatic);

        private static readonly MethodInfo _excludeByIdMethod = typeof(EnumerableExtensions)
            .GetMethod("ExcludeById", Constants.PublicStatic);

        private static readonly MethodInfo _excludeMethod = typeof(EnumerableExtensions)
            .GetMethod("Exclude", Constants.PublicStatic);

        private static readonly MethodInfo _forEachMethod = typeof(EnumerableExtensions)
            .GetMethods(Constants.PublicStatic)
            .First(m => m.Name == "ForEach");

        private static readonly MethodInfo _forEachTupleMethod = typeof(EnumerableExtensions)
            .GetMethods(Constants.PublicStatic)
            .Last(m => m.Name == "ForEach");

        #endregion

        private readonly ParameterExpression _sourceElementParameter;
        private readonly Type _sourceElementType;
        private readonly LambdaExpression _sourceElementIdLambda;
        private readonly Type _targetElementType;
        private readonly LambdaExpression _targetElementIdLambda;
        private readonly bool _elementsAreAssignable;
        private Expression _population;

        public EnumerablePopulationBuilder(IObjectMappingContext omc)
        {
            ObjectMappingContext = omc;

            _sourceElementType = omc.SourceType.GetEnumerableElementType();
            _sourceElementParameter = GetParameter(_sourceElementType, omc);

            _targetElementType = TargetCollectionType.GetEnumerableElementType();
            var targetElementParameter = GetParameter(_targetElementType, omc);

            var sourceElementId = GetIdentifierOrNull(_sourceElementType, _sourceElementParameter, omc);
            var targetElementId = GetIdentifierOrNull(_targetElementType, targetElementParameter, omc);
            _sourceElementIdLambda = GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, targetElementId);
            _targetElementIdLambda = GetTargetElementIdLambda(targetElementParameter, targetElementId);

            _elementsAreAssignable = _targetElementType.IsAssignableFrom(_sourceElementType);

            SetPopulationToDefault();
        }

        #region Setup

        private static ParameterExpression GetParameter(Type type, IObjectMappingContext omc)
            => omc.GlobalContext.Cache.GetOrAdd(type.FullName + ": EnumerableParameter", k => Parameters.Create(type));

        private static Expression GetIdentifierOrNull(Type type, Expression parameter, IObjectMappingContext omc)
        {
            return omc.MapperContext.Cache.GetOrAdd(TypeIdentifierKey.For(type).Value, k =>
            {
                var configuredIdentifier = omc.MapperContext.UserConfigurations.Identifiers.GetIdentifierOrNullFor(type);

                if (configuredIdentifier != null)
                {
                    return configuredIdentifier.ReplaceParameterWith(parameter);
                }

                var identifier = omc.GlobalContext.MemberFinder.GetIdentifierOrNull(type);

                return identifier?.GetAccess(parameter);
            });
        }

        private LambdaExpression GetSourceElementIdLambda(
            ParameterExpression sourceElement,
            Expression sourceElementId,
            Expression targetElementId)
        {
            if ((sourceElementId == null) || (targetElementId == null))
            {
                return null;
            }

            return Expression.Lambda(
                Expression.GetFuncType(sourceElement.Type, targetElementId.Type),
                GetSimpleElementConversion(sourceElementId, targetElementId.Type),
                sourceElement);
        }

        private static LambdaExpression GetTargetElementIdLambda(ParameterExpression targetElement, Expression targetElementId)
        {
            if (targetElementId == null)
            {
                return null;
            }

            return Expression.Lambda(
                Expression.GetFuncType(targetElement.Type, targetElementId.Type),
                targetElementId,
                targetElement);
        }

        private void SetPopulationToDefault()
        {
            _population = ObjectMappingContext.SourceObject;
        }

        #endregion

        #region Operator

        public static implicit operator Expression(EnumerablePopulationBuilder builder)
        {
            var population = builder._population;

            builder.SetPopulationToDefault();

            return population;
        }

        #endregion

        public IObjectMappingContext ObjectMappingContext { get; }

        public bool TypesAreIdentifiable => (_sourceElementIdLambda != null) && (_targetElementIdLambda != null);

        private Type TargetCollectionType => ObjectMappingContext.InstanceVariable.Type;

        public EnumerablePopulationBuilder IntersectTargetById()
        {
            var typedIntersectByIdMethod = _intersectByIdMethod
                .MakeGenericMethod(_sourceElementType, _targetElementType, _targetElementIdLambda.ReturnType);

            var intersectByIdCall = Expression.Call(
                typedIntersectByIdMethod,
                ObjectMappingContext.SourceObject,
                ObjectMappingContext.InstanceVariable,
                _sourceElementIdLambda,
                _targetElementIdLambda);

            _population = intersectByIdCall;
            return this;
        }

        public EnumerablePopulationBuilder ProjectToTargetType()
        {
            if (_elementsAreAssignable && _targetElementType.IsSimple())
            {
                _population = ObjectMappingContext.SourceObject;
                return this;
            }

            var typedSelectMethod = _selectMethod
                .MakeGenericMethod(_sourceElementType, _targetElementType);

            var projectionFunc = Expression.Lambda(
                Expression.GetFuncType(_sourceElementType, typeof(int), _targetElementType),
                GetElementConversion(),
                _sourceElementParameter,
                Parameters.EnumerableIndex);

            var selectCall = Expression.Call(
                typedSelectMethod,
                _population,
                projectionFunc);

            _population = selectCall;
            return this;
        }

        private Expression GetElementConversion()
        {
            return _sourceElementType.IsSimple()
                ? GetSimpleElementConversion(_sourceElementParameter, _targetElementType)
                : GetMapElementCall(_sourceElementParameter, Expression.Default(_targetElementType));
        }

        private Expression GetSimpleElementConversion(Expression sourceElement, Type targetType)
            => ObjectMappingContext.MapperContext.ValueConverters.GetConversion(sourceElement, targetType);

        private Expression GetMapElementCall(Expression sourceObject, Expression existingObject)
            => ObjectMappingContext.GetMapCall(sourceObject, existingObject);

        public EnumerablePopulationBuilder ExcludeSourceById()
        {
            var typedExcludeByIdMethod = _excludeByIdMethod.MakeGenericMethod(
                _targetElementType,
                _sourceElementType,
                _sourceElementIdLambda.ReturnType);

            var excludeByIdCall = Expression.Call(
                typedExcludeByIdMethod,
                ObjectMappingContext.InstanceVariable,
                _population,
                _targetElementIdLambda,
                _sourceElementIdLambda);

            _population = excludeByIdCall;
            return this;
        }

        public EnumerablePopulationBuilder ExcludeTargetById()
        {
            var typedExcludeByIdMethod = _excludeByIdMethod.MakeGenericMethod(
                _sourceElementType,
                _targetElementType,
                _targetElementIdLambda.ReturnType);

            var excludeByIdCall = Expression.Call(
                typedExcludeByIdMethod,
                _population,
                ObjectMappingContext.InstanceVariable,
                _sourceElementIdLambda,
                _targetElementIdLambda);

            _population = excludeByIdCall;
            return this;
        }

        public EnumerablePopulationBuilder ExcludeTarget()
        {
            _population = Expression.Call(
                _excludeMethod.MakeGenericMethod(_targetElementType),
                _population,
                ObjectMappingContext.InstanceVariable);

            return this;
        }

        public Expression ClearTarget()
        {
            _population = Expression.Call(
                ObjectMappingContext.InstanceVariable,
                TargetCollectionType.GetMethod("Clear", Constants.PublicInstance));

            return this;
        }

        public EnumerablePopulationBuilder CallToArray()
        {
            _population = _population.WithToArrayCall();
            return this;
        }

        public Expression AddResultsToTarget()
        {
            _population = GetForEachCall(_population, p => GetTargetMethodCall("Add", p));
            return this;
        }

        public Expression MapResultsToTarget()
        {
            var typedForEachMethod = _forEachTupleMethod.MakeGenericMethod(_sourceElementType, _targetElementType);
            var forEachActionType = Expression.GetActionType(_sourceElementType, _targetElementType, typeof(int));
            var sourceElementParameter = Parameters.Create(_sourceElementType);
            var targetElementParameter = Parameters.Create(_targetElementType);
            var forEachAction = GetMapElementCall(sourceElementParameter, targetElementParameter);

            var forEachLambda = Expression.Lambda(
                forEachActionType,
                forEachAction,
                sourceElementParameter,
                targetElementParameter,
                Parameters.EnumerableIndex);

            var forEachCall = Expression.Call(typedForEachMethod, _population, forEachLambda);

            _population = forEachCall;
            return this;
        }

        public Expression RemoveResultsFromTarget()
        {
            _population = GetForEachCall(_population, p => GetTargetMethodCall("Remove", p));
            return this;
        }

        private Expression GetTargetMethodCall(string methodName, Expression argument)
        {
            return Expression.Call(
                ObjectMappingContext.InstanceVariable,
                TargetCollectionType.GetMethod(methodName, Constants.PublicInstance),
                argument);
        }

        private static Expression GetForEachCall(Expression subject, Func<Expression, Expression> forEachActionFactory)
        {
            var elementType = subject.Type.GetEnumerableElementType();
            var typedForEachMethod = _forEachMethod.MakeGenericMethod(elementType);
            var forEachActionType = Expression.GetActionType(elementType);
            var parameter = Parameters.Create(elementType);
            var forEachAction = forEachActionFactory.Invoke(parameter);
            var forEachLambda = Expression.Lambda(forEachActionType, forEachAction, parameter);
            var forEachCall = Expression.Call(typedForEachMethod, subject, forEachLambda);

            return forEachCall;
        }
    }
}