namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Caching;
    using Extensions;
    using Members;

    internal class EnumerablePopulationBuilder
    {
        #region Cached Items

        private static readonly MethodInfo _concatMethod = typeof(Enumerable)
            .GetPublicStaticMethod("Concat");

        private static readonly MethodInfo _selectMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .Last(m => (m.Name == "Select") && m.GetParameters().Length == 2);

        private static readonly MethodInfo _excludeMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethod("Exclude");

        private static readonly MethodInfo _forEachMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods()
            .First(m => m.Name == "ForEach");

        private static readonly MethodInfo _forEachTupleMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods()
            .Last(m => m.Name == "ForEach");

        #endregion

        private readonly ObjectMapperData _omd;
        private readonly ICache<TypeKey, Expression> _typeIdsCache;
        private readonly ICache<TypeKey, ParameterExpression> _parametersCache;
        private readonly EnumerableTypeHelper _typeHelper;
        private readonly ParameterExpression _sourceElementParameter;
        private readonly Type _sourceElementType;
        private readonly LambdaExpression _sourceElementIdLambda;
        private readonly Type _targetElementType;
        private readonly LambdaExpression _targetElementIdLambda;
        private readonly bool _elementsAreAssignable;
        private readonly ICollection<Expression> _populationExpressions;
        private Expression _population;
        private ParameterExpression _collectionDataVariable;

        public EnumerablePopulationBuilder(ObjectMapperData omd)
        {
            _omd = omd;
            _typeIdsCache = omd.MapperContext.Cache.CreateScoped<TypeKey, Expression>();
            _parametersCache = GlobalContext.Instance.Cache.CreateScoped<TypeKey, ParameterExpression>();
            _typeHelper = new EnumerableTypeHelper(omd.TargetMember.Type, omd.TargetMember.ElementType);

            _sourceElementType = omd.SourceType.GetEnumerableElementType();
            _targetElementType = _typeHelper.ElementType;

            _sourceElementParameter = GetParameter(_sourceElementType);
            var sourceElementId = GetIdentifierOrNull(_sourceElementType, _sourceElementParameter, omd);

            if (_sourceElementType == _targetElementType)
            {
                _sourceElementIdLambda =
                    _targetElementIdLambda =
                        GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, sourceElementId);

                _elementsAreAssignable = true;
            }
            else
            {
                var targetElementParameter = GetParameter(_targetElementType);
                var targetElementId = GetIdentifierOrNull(_targetElementType, targetElementParameter, omd);
                _sourceElementIdLambda = GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, targetElementId);
                _targetElementIdLambda = GetTargetElementIdLambda(targetElementParameter, targetElementId);

                _elementsAreAssignable = _targetElementType.IsAssignableFrom(_sourceElementType);
            }

            _populationExpressions = new List<Expression>();

            SetPopulationToDefault();
        }

        #region Setup

        private ParameterExpression GetParameter(Type type)
            => _parametersCache.GetOrAdd(TypeKey.ForParameter(type), key => Parameters.Create(key.Type));

        private Expression GetIdentifierOrNull(Type type, Expression parameter, MemberMapperData data)
        {
            return _typeIdsCache.GetOrAdd(TypeKey.ForTypeId(type), key =>
            {
                var configuredIdentifier = data.MapperContext.UserConfigurations.Identifiers.GetIdentifierOrNullFor(key.Type);

                if (configuredIdentifier != null)
                {
                    return configuredIdentifier.ReplaceParameterWith(parameter);
                }

                var identifier = GlobalContext.Instance.MemberFinder.GetIdentifierOrNull(key);

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
            _population = _omd.SourceObject;
        }

        #endregion

        #region Operator

        public static implicit operator Expression(EnumerablePopulationBuilder builder)
        {
            var population = (builder._collectionDataVariable != null)
                ? Expression.Block(new[] { builder._collectionDataVariable }, builder._populationExpressions)
                : Expression.Block(builder._populationExpressions);

            builder.SetPopulationToDefault();

            return population;
        }

        #endregion

        public bool TypesAreIdentifiable => (_sourceElementIdLambda != null) && (_targetElementIdLambda != null);

        public EnumerablePopulationBuilder ProjectToTargetType()
        {
            if (_elementsAreAssignable && _targetElementType.IsSimple())
            {
                _population = _omd.SourceObject;
                return this;
            }

            var typedSelectMethod = _selectMethod
                .MakeGenericMethod(_sourceElementType, _targetElementType);

            var subject = (_collectionDataVariable != null)
                ? Expression.Property(_collectionDataVariable, "NewSourceItems")
                : _population;

            var projectionFunc = Expression.Lambda(
                Expression.GetFuncType(_sourceElementType, typeof(int), _targetElementType),
                GetElementConversion(),
                _sourceElementParameter,
                Parameters.EnumerableIndex);

            var selectCall = Expression.Call(
                typedSelectMethod,
                subject,
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
            => _omd.MapperContext.ValueConverters.GetConversion(sourceElement, targetType);

        private Expression GetMapElementCall(Expression sourceObject, Expression existingObject)
            => _omd.GetMapCall(sourceObject, existingObject);

        public void AssignValueToVariable() => _populationExpressions.Add(GetVariableAssignment());

        public void IfTargetNotNull(params Func<EnumerablePopulationBuilder, Expression>[] nonNullTargetActionFactories)
        {
            var targetNotNull = _omd.TargetObject.GetIsNotDefaultComparison();

            var notNullActions = (nonNullTargetActionFactories.Length == 1)
                ? nonNullTargetActionFactories[0].Invoke(this)
                : Expression.Block(nonNullTargetActionFactories.Select(af => af.Invoke(this)));

            var ifNotNullPerformActions = Expression.IfThen(targetNotNull, notNullActions);
            _populationExpressions.Add(ifNotNullPerformActions);
        }

        public Expression AddVariableToTarget()
        {
            if (_typeHelper.IsList)
            {
                return GetTargetMethodCall("AddRange", _omd.InstanceVariable);
            }

            if (!(_typeHelper.IsArray || _typeHelper.IsEnumerableInterface))
            {
                return GetForEachCall(
                    _omd.InstanceVariable.WithToArrayCall(_targetElementType),
                    p => GetTargetMethodCall("Add", p));
            }

            _population = GetTargetMethodCall(
                _concatMethod.MakeGenericMethod(_targetElementType),
                _omd.InstanceVariable);

            var reassignment = GetVariableAssignment();

            return reassignment;
        }

        public Expression RemoveTargetItemsById()
        {
            var absentTargetItems = Expression.Property(_collectionDataVariable, "AbsentTargetItems");

            if (!(_typeHelper.IsArray || _typeHelper.IsEnumerableInterface))
            {
                var removeExistingItems = GetForEachCall(absentTargetItems, p => GetTargetMethodCall("Remove", p));
                var addNewItems = AddVariableToTarget();

                return Expression.Block(removeExistingItems, addNewItems);
            }

            var excludeCall = Expression.Call(
                _excludeMethod.MakeGenericMethod(_targetElementType),
                _omd.TargetObject,
                absentTargetItems);

            var concatCall = Expression.Call(
                _concatMethod.MakeGenericMethod(_targetElementType),
                excludeCall,
                _omd.InstanceVariable);

            _population = concatCall;

            var reassignment = GetVariableAssignment();

            return reassignment;
        }

        public void CreateCollectionData()
        {
            var createCollectionMethod = CollectionData
                .CreateMethod
                .MakeGenericMethod(_sourceElementType, _targetElementType, _targetElementIdLambda.ReturnType);

            var createCollectionDataCall = Expression.Call(
                createCollectionMethod,
                _population,
                _omd.TargetObject,
                _sourceElementIdLambda,
                _targetElementIdLambda);

            _collectionDataVariable = Parameters.Create(
                typeof(CollectionData<,>).MakeGenericType(_sourceElementType, _targetElementType),
                "collectionData");

            var assignCollectionData = Expression.Assign(_collectionDataVariable, createCollectionDataCall);

            _populationExpressions.Add(assignCollectionData);
        }

        public Expression MapIntersection()
        {
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

            var forEachCall = Expression.Call(
                _forEachTupleMethod.MakeGenericMethod(_sourceElementType, _targetElementType),
                Expression.Property(_collectionDataVariable, "Intersection"),
                forEachLambda);

            return forEachCall;
        }

        public EnumerablePopulationBuilder ExcludeTarget()
        {
            _population = Expression.Call(
                _excludeMethod.MakeGenericMethod(_targetElementType),
                _population,
                _omd.TargetObject);

            return this;
        }

        public Expression ReplaceTargetItems()
        {
            if (_typeHelper.IsArray || _typeHelper.IsEnumerableInterface)
            {
                return Constants.EmptyExpression;
            }

            var clearExistingCollection = Expression.Call(_omd.TargetObject, GetTargetMethod("Clear"));
            var addNewItems = AddVariableToTarget();

            return Expression.Block(clearExistingCollection, addNewItems);
        }

        public Expression ExistingOrNewEmptyInstance()
        {
            var emptyInstance = _omd.TargetMember.GetEmptyInstanceCreation();

            return Expression.Coalesce(_omd.TargetObject, emptyInstance);
        }

        public Expression GetReturnValue()
        {
            if (_typeHelper.IsArray)
            {
                return _omd.InstanceVariable.WithToArrayCall(_targetElementType);
            }

            var instanceToList = _omd.InstanceVariable.WithToListCall(_targetElementType);

            if (_typeHelper.IsCollection)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var newCollection = Expression.New(
                    _typeHelper.CollectionType.GetConstructor(new[] { _typeHelper.ListType }),
                    instanceToList);

                return Expression.Coalesce(_omd.TargetObject, newCollection);
            }

            if (_typeHelper.IsList)
            {
                return Expression.Coalesce(_omd.TargetObject, instanceToList);
            }

            return instanceToList;
        }

        private Expression GetVariableAssignment() => Expression.Assign(_omd.InstanceVariable, _population);

        private Expression GetTargetMethodCall(string methodName, Expression argument)
            => GetTargetMethodCall(GetTargetMethod(methodName), argument);

        private Expression GetTargetMethodCall(MethodInfo method, Expression argument)
        {
            return method.IsStatic
                ? Expression.Call(method, _omd.TargetObject, argument)
                : Expression.Call(_omd.TargetObject, method, argument);
        }

        private MethodInfo GetTargetMethod(string methodName) => _omd.TargetObject.Type.GetMethod(methodName);

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