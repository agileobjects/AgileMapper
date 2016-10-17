namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections;
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

        private static readonly MethodInfo _enumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        private static readonly MethodInfo _disposeMethod = typeof(IDisposable).GetMethod("Dispose");

        #endregion

        private readonly ObjectMapperData _omd;
        private readonly ICache<TypeKey, Expression> _typeIdsCache;
        private readonly ICache<TypeKey, ParameterExpression> _parametersCache;
        private readonly SourceItemsSelector _sourceItemsSelector;
        private EnumerableTypeHelper _sourceTypeHelper;
        private readonly EnumerableTypeHelper _targetTypeHelper;
        private readonly ParameterExpression _sourceElementParameter;
        private readonly string _sourceVariableName;
        private ParameterExpression _sourceVariable;
        private readonly Type _sourceElementType;
        private readonly LambdaExpression _sourceElementIdLambda;
        private readonly Type _targetElementType;
        private readonly LambdaExpression _targetElementIdLambda;
        private readonly bool _discardExistingValues;
        private readonly ICollection<Expression> _populationExpressions;
        private bool _hasNullTargetShortCircuit;
        private ParameterExpression _collectionDataVariable;

        public EnumerablePopulationBuilder(ObjectMapperData omd)
        {
            _omd = omd;
            _typeIdsCache = omd.MapperContext.Cache.CreateScoped<TypeKey, Expression>();
            _parametersCache = GlobalContext.Instance.Cache.CreateScoped<TypeKey, ParameterExpression>();
            _sourceItemsSelector = new SourceItemsSelector(this);

            _sourceElementType = omd.SourceType.GetEnumerableElementType();
            _targetTypeHelper = new EnumerableTypeHelper(omd.TargetType, omd.TargetMember.ElementType);
            _targetElementType = _targetTypeHelper.ElementType;
            ElementTypesAreTheSame = _sourceElementType == _targetElementType;
            ElementTypesAreSimple = _targetElementType.IsSimple();

            _sourceElementParameter = GetParameter(_sourceElementType);
            var sourceElementId = GetIdentifierOrNull(_sourceElementType, _sourceElementParameter, omd);
            string targetVariableName;

            if (ElementTypesAreTheSame)
            {
                _sourceElementIdLambda =
                    _targetElementIdLambda =
                        GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, sourceElementId);

                _sourceVariableName = "source" + _omd.TargetType.GetVariableName();
                targetVariableName = "target" + _omd.TargetType.GetVariableName();
            }
            else
            {
                var targetElementParameter = GetParameter(_targetElementType);
                var targetElementId = GetIdentifierOrNull(_targetElementType, targetElementParameter, omd);
                _sourceElementIdLambda = GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, targetElementId);
                _targetElementIdLambda = GetTargetElementIdLambda(targetElementParameter, targetElementId);

                _sourceVariableName = _omd.SourceType.GetVariableName(f => f.InCamelCase);
                targetVariableName = _omd.TargetType.GetVariableName(f => f.InCamelCase);
            }

            _discardExistingValues = omd.RuleSet.EnumerablePopulationStrategy.DiscardExistingValues;
            TargetIsReadOnly = _targetTypeHelper.IsArray || _targetTypeHelper.IsEnumerableInterface;
            TargetVariable = GetTargetVariable(targetVariableName);

            _populationExpressions = new List<Expression>();
        }

        #region Setup

        private ParameterExpression GetParameter(Type type)
            => _parametersCache.GetOrAdd(TypeKey.ForParameter(type), key => Parameters.Create(key.Type));

        private Expression GetIdentifierOrNull(Type type, Expression parameter, IMemberMapperData mapperData)
        {
            return _typeIdsCache.GetOrAdd(TypeKey.ForTypeId(type), key =>
            {
                var configuredIdentifier =
                    mapperData.MapperContext.UserConfigurations.Identifiers.GetIdentifierOrNullFor(key.Type);

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

        private ParameterExpression GetTargetVariable(string name)
        {
            var targetVariableType = ElementTypesAreSimple && TargetIsReadOnly && _discardExistingValues
                ? _omd.TargetType
                : _targetTypeHelper.IsList
                    ? _targetTypeHelper.ListType
                    : typeof(ICollection<>).MakeGenericType(_targetElementType);

            return Expression.Variable(targetVariableType, name);
        }

        #endregion

        #region Operator

        public static implicit operator Expression(EnumerablePopulationBuilder builder)
        {
            var variables = new List<ParameterExpression>(2);

            if (builder._sourceVariable != null)
            {
                variables.Add(builder._sourceVariable);
            }

            if (builder._collectionDataVariable != null)
            {
                variables.Add(builder._collectionDataVariable);
            }

            var population = variables.Any()
                ? Expression.Block(variables, builder._populationExpressions)
                : Expression.Block(builder._populationExpressions);

            return population;
        }

        #endregion

        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreIdentifiable => (_sourceElementIdLambda != null) && (_targetElementIdLambda != null);

        public bool ElementTypesAreSimple { get; }

        public bool TargetIsReadOnly { get; }

        public ParameterExpression TargetVariable { get; }

        public void PopulateTargetVariableFromSourceObjectOnly()
        {
            _populationExpressions.Add(Expression.Assign(TargetVariable, GetSourceOnlyReturnValue()));
        }

        public void AssignSourceVariableFromSourceObject() => AssignSourceVariableFrom(_omd.SourceObject);

        public void AssignSourceVariableFrom(Func<SourceItemsSelector, SourceItemsSelector> sourceItemsSelection)
            => AssignSourceVariableFrom(sourceItemsSelection.Invoke(_sourceItemsSelector).GetResult());

        private void AssignSourceVariableFrom(Expression sourceValue)
        {
            _sourceTypeHelper = new EnumerableTypeHelper(
                sourceValue.Type,
                ElementTypesAreTheSame ? _sourceElementType : sourceValue.Type.GetEnumerableElementType());

            _sourceVariable = Expression.Variable(sourceValue.Type, _sourceVariableName);
            var sourceVariableAssignment = Expression.Assign(_sourceVariable, sourceValue);

            _populationExpressions.Add(sourceVariableAssignment);
        }

        public void AssignTargetVariable()
        {
            var targetVariableValue = GetTargetVariableValue();

            _populationExpressions.Add(Expression.Assign(TargetVariable, targetVariableValue));
        }

        private Expression GetTargetVariableValue()
        {
            Expression nonNullTargetVariableValue;

            if (TargetIsReadOnly)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                nonNullTargetVariableValue = Expression.New(
                    _targetTypeHelper.ListType.GetConstructor(new[] { _targetTypeHelper.EnumerableInterfaceType }),
                    _omd.TargetObject);
            }
            else
            {
                nonNullTargetVariableValue = _omd.TargetObject;
            }

            if (_hasNullTargetShortCircuit)
            {
                return nonNullTargetVariableValue;
            }

            var targetVariableValue = Expression.Condition(
                _omd.TargetObject.GetIsNotDefaultComparison(),
                nonNullTargetVariableValue,
                Expression.New(_targetTypeHelper.ListType),
                TargetVariable.Type);

            return targetVariableValue;
        }

        public void RemoveAllTargetItems()
        {
            _populationExpressions.Add(GetTargetMethodCall("Clear"));
        }

        public void AddNewItemsToTargetVariable()
        {
            if (ElementTypesAreSimple && ElementTypesAreTheSame && _targetTypeHelper.IsList)
            {
                _populationExpressions.Add(GetTargetMethodCall("AddRange", _sourceVariable));
                return;
            }

            var counter = Parameters.EnumerableIndex;

            Func<Expression, Expression> populationLoopAdapter;
            Expression loopExitCheck, sourceElement;

            if (_sourceTypeHelper.IsListInterface)
            {
                populationLoopAdapter = exp => exp;
                loopExitCheck = GetCounterEqualsCountCheck(counter);
                sourceElement = GetIndexedElementAccess(counter);
            }
            else
            {
                ParameterExpression enumerator;
                var enumeratorAssignment = GetEnumeratorAssignment(out enumerator);

                loopExitCheck = Expression.Not(Expression.Call(enumerator, _enumeratorMoveNextMethod));
                sourceElement = Expression.Property(enumerator, "Current");

                populationLoopAdapter = loop => Expression.Block(
                    new[] { enumerator },
                    enumeratorAssignment,
                    Expression.TryFinally(loop, Expression.Call(enumerator, _disposeMethod)));
            }

            var populationLoop = GetPopulationLoop(sourceElement, loopExitCheck, populationLoopAdapter);

            var population = Expression.Block(
                new[] { counter },
                Expression.Assign(counter, Expression.Constant(0)),
                populationLoop);

            _populationExpressions.Add(population);
        }

        private Expression GetCounterEqualsCountCheck(Expression counter)
        {
            var countPropertyName = _sourceTypeHelper.IsArray ? "Length" : "Count";
            var countProperty = Expression.Property(_sourceVariable, countPropertyName);

            return Expression.Equal(counter, countProperty);
        }

        private Expression GetIndexedElementAccess(Expression counter)
        {
            if (_sourceTypeHelper.IsArray)
            {
                return Expression.ArrayIndex(_sourceVariable, counter);
            }

            var indexer = _sourceVariable.Type
                .GetPublicInstanceProperties()
                .First(p =>
                    (p.GetIndexParameters().Length == 1) &&
                    (p.GetIndexParameters()[0].ParameterType == typeof(int)));

            return Expression.MakeIndex(_sourceVariable, indexer, new[] { counter });
        }

        private Expression GetEnumeratorAssignment(out ParameterExpression enumerator)
        {
            var getEnumeratorMethod = _sourceTypeHelper.EnumerableInterfaceType.GetMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(_sourceVariable, getEnumeratorMethod);
            enumerator = Expression.Variable(getEnumeratorCall.Type, "enumerator");
            var enumeratorAssignment = Expression.Assign(enumerator, getEnumeratorCall);

            return enumeratorAssignment;
        }

        private Expression GetPopulationLoop(
            Expression sourceElement,
            Expression loopExitCheck,
            Func<Expression, Expression> populationLoopAdapter)
        {
            var breakLoop = Expression.Break(Expression.Label(typeof(void), "Break"));

            var elementToAdd = GetElementConversion(sourceElement);
            var addMappedElement = GetTargetMethodCall("Add", elementToAdd);

            var loopBody = Expression.Block(
                Expression.IfThen(loopExitCheck, breakLoop),
                addMappedElement,
                Expression.PreIncrementAssign(Parameters.EnumerableIndex));

            var populationLoop = Expression.Loop(loopBody, breakLoop.Target);
            var adaptedLoop = populationLoopAdapter.Invoke(populationLoop);

            return adaptedLoop;
        }

        private Expression GetElementConversion(Expression sourceElement)
        {
            return sourceElement.Type.IsSimple()
                ? GetSimpleElementConversion(sourceElement, _targetElementType)
                : GetMapElementCall(sourceElement, Expression.Default(_targetElementType));
        }

        private Expression GetSimpleElementConversion(Expression sourceElement, Type targetType)
            => _omd.MapperContext.ValueConverters.GetConversion(sourceElement, targetType);

        private Expression GetMapElementCall(Expression sourceObject, Expression existingObject)
            => _omd.GetMapCall(sourceObject, existingObject);

        public void AddNullTargetShortCircuitIfAppropriate()
        {
            if (ElementTypesAreSimple)
            {
                AddNullTargetShortCircuit();
            }
        }

        public void AddNullTargetShortCircuit()
        {
            var returnValue = GetSourceOnlyReturnValue();

            var ifTargetNullShortCircuit = Expression.IfThen(
                _omd.TargetObject.GetIsDefaultComparison(),
                Expression.Return(_omd.ReturnLabelTarget, returnValue));

            _populationExpressions.Add(ifTargetNullShortCircuit);

            _hasNullTargetShortCircuit = true;
        }

        private Expression GetSourceOnlyReturnValue()
        {
            var convertedSourceItems = _sourceItemsSelector.SourceItemsProjectedToTargetType().GetResult();
            var returnValue = ConvertForReturnValue(convertedSourceItems);

            return returnValue;
        }

        public void CreateCollectionData()
        {
            var createCollectionDataMethod = ElementTypesAreTheSame
                ? CollectionData.IdSameTypesCreateMethod
                    .MakeGenericMethod(_targetElementType, _targetElementIdLambda.ReturnType)
                : CollectionData.IdDifferentTypesCreateMethod
                    .MakeGenericMethod(_sourceElementType, _targetElementType, _targetElementIdLambda.ReturnType);

            var callArguments = new List<Expression>(4) { _omd.SourceObject, _omd.TargetObject, _sourceElementIdLambda };

            if (!ElementTypesAreTheSame)
            {
                callArguments.Add(_targetElementIdLambda);
            }

            var createCollectionDataCall = Expression.Call(createCollectionDataMethod, callArguments);

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

            _populationExpressions.Add(forEachCall);

            return forEachCall;
        }

        public void RemoveTargetItemsById()
        {
            var absentTargetItems = Expression.Property(_collectionDataVariable, "AbsentTargetItems");
            var removeExistingItems = GetForEachCall(absentTargetItems, p => GetTargetMethodCall("Remove", p));

            _populationExpressions.Add(removeExistingItems);
        }

        public Expression ExistingOrNewEmptyInstance()
        {
            var emptyInstance = _omd.TargetMember.GetEmptyInstanceCreation();

            return Expression.Coalesce(_omd.TargetObject, emptyInstance);
        }

        public Expression GetReturnValue() => ConvertForReturnValue(TargetVariable);

        private Expression ConvertForReturnValue(Expression value)
        {
            var allowSameValue = value.NodeType != ExpressionType.MemberAccess;

            if (allowSameValue && _omd.TargetType.IsAssignableFrom(value.Type))
            {
                return value;
            }

            if (_targetTypeHelper.IsArray)
            {
                return value.WithToArrayCall(_targetElementType);
            }

            if (_targetTypeHelper.IsList)
            {
                return allowSameValue
                    ? value.GetConversionTo(_targetTypeHelper.ListType)
                    : value.WithToListCall(_targetElementType);
            }

            if (_targetTypeHelper.IsCollection)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var newCollection = Expression.New(
                    _targetTypeHelper.CollectionType.GetConstructor(new[] { _targetTypeHelper.ListType }),
                    value.GetConversionTo(_targetTypeHelper.ListType));

                return Expression.Coalesce(_omd.TargetObject, newCollection);
            }

            return value.WithToListCall(_targetElementType);
        }

        private Expression GetTargetMethodCall(string methodName, Expression argument = null)
        {
            var method = TargetVariable.Type.GetMethod(methodName);

            return (argument != null)
                ? Expression.Call(TargetVariable, method, argument)
                : Expression.Call(TargetVariable, method);
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

        public class SourceItemsSelector
        {
            private readonly EnumerablePopulationBuilder _builder;
            private Expression _result;

            internal SourceItemsSelector(EnumerablePopulationBuilder builder)
            {
                _builder = builder;
            }

            public SourceItemsSelector SourceItems()
            {
                _result = _builder._omd.SourceObject;
                return this;
            }

            public SourceItemsSelector SourceItemsProjectedToTargetType()
            {
                if (_builder.ElementTypesAreTheSame)
                {
                    _result = _builder._omd.SourceObject;
                    return this;
                }

                var typedSelectMethod = _selectMethod
                    .MakeGenericMethod(_builder._sourceElementType, _builder._targetElementType);

                var projectionFunc = Expression.Lambda(
                    Expression.GetFuncType(_builder._sourceElementType, typeof(int), _builder._targetElementType),
                    _builder.GetElementConversion(_builder._sourceElementParameter),
                    _builder._sourceElementParameter,
                    Parameters.EnumerableIndex);

                _result = Expression.Call(typedSelectMethod, _builder._omd.SourceObject, projectionFunc);

                return this;
            }

            public SourceItemsSelector ExcludingTargetItems()
            {
                _result = Expression.Call(
                    _excludeMethod.MakeGenericMethod(_builder._targetElementType),
                    _result,
                    _builder._omd.TargetObject);

                return this;
            }

            public SourceItemsSelector CollectionDataNewSourceItems()
            {
                _result = Expression.Property(_builder._collectionDataVariable, "NewSourceItems");
                return this;
            }

            public Expression GetResult()
            {
                if (_result.NodeType == ExpressionType.MemberAccess)
                {
                    return _result;
                }

                _result = _builder._targetTypeHelper.IsArray
                    ? _result.WithToArrayCall(_builder._targetElementType)
                    : _result.WithToListCall(_builder._targetElementType);

                return _result;
            }
        }
    }
}