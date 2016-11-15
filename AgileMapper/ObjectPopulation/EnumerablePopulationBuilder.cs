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
    using NetStandardPolyfills;

    internal class EnumerablePopulationBuilder
    {
        #region Untyped MethodInfos

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
        private readonly SourceItemsSelector _sourceItemsSelector;
        private EnumerableTypeHelper _sourceTypeHelper;
        private readonly EnumerableTypeHelper _targetTypeHelper;
        private readonly ParameterExpression _sourceElementParameter;
        private ParameterExpression _sourceVariable;
        private readonly Type _sourceElementType;
        private readonly LambdaExpression _sourceElementIdLambda;
        private readonly Type _targetElementType;
        private readonly LambdaExpression _targetElementIdLambda;
        private readonly ICollection<Expression> _populationExpressions;
        private ParameterExpression _collectionDataVariable;
        private ParameterExpression _counterVariable;

        public EnumerablePopulationBuilder(ObjectMapperData omd)
        {
            _omd = omd;
            _sourceItemsSelector = new SourceItemsSelector(this);

            var typeIdsCache = omd.MapperContext.Cache.CreateScoped<TypeKey, Expression>();

            _sourceElementType = omd.SourceType.GetEnumerableElementType();
            _targetTypeHelper = new EnumerableTypeHelper(omd.TargetType, omd.TargetMember.ElementType);
            _targetElementType = _targetTypeHelper.ElementType;
            ElementTypesAreTheSame = _sourceElementType == _targetElementType;
            ElementTypesAreSimple = _targetElementType.IsSimple();

            _sourceElementParameter = _sourceElementType.GetOrCreateParameter();
            var sourceElementId = GetIdentifierOrNull(_sourceElementType, _sourceElementParameter, omd, typeIdsCache);

            if (ElementTypesAreTheSame)
            {
                _sourceElementIdLambda =
                    _targetElementIdLambda =
                        GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, sourceElementId);
            }
            else
            {
                var targetElementParameter = _targetElementType.GetOrCreateParameter();
                var targetElementId = GetIdentifierOrNull(_targetElementType, targetElementParameter, omd, typeIdsCache);
                _sourceElementIdLambda = GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, targetElementId);
                _targetElementIdLambda = GetTargetElementIdLambda(targetElementParameter, targetElementId);
            }

            _populationExpressions = new List<Expression>();
        }

        #region Setup

        private Expression GetIdentifierOrNull(
            Type type,
            Expression parameter,
            IMemberMapperData mapperData,
            ICache<TypeKey, Expression> cache)
        {
            if (ElementTypesAreSimple)
            {
                return null;
            }

            return cache.GetOrAdd(TypeKey.ForTypeId(type), key =>
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

        public ParameterExpression Counter => _counterVariable ?? (_counterVariable = GetCounterVariable());

        private ParameterExpression GetCounterVariable()
        {
            if (_omd.IsRoot)
            {
                return Parameters.Create<int>("i");
            }

            var counterName = 'i';

            var parentMapperData = _omd.Parent;

            while (!parentMapperData.Context.IsStandalone)
            {
                if (parentMapperData.TargetMember.IsEnumerable)
                {
                    ++counterName;
                }

                parentMapperData = parentMapperData.Parent;
            }

            return Parameters.Create<int>(counterName.ToString());
        }

        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreIdentifiable => (_sourceElementIdLambda != null) && (_targetElementIdLambda != null);

        public bool ElementTypesAreSimple { get; }

        public ParameterExpression TargetVariable { get; private set; }

        public void AssignSourceVariableFromSourceObject() => AssignSourceVariableFrom(_omd.SourceObject);

        public void AssignSourceVariableFrom(Func<SourceItemsSelector, SourceItemsSelector> sourceItemsSelection)
            => AssignSourceVariableFrom(sourceItemsSelection.Invoke(_sourceItemsSelector).GetResult());

        private void AssignSourceVariableFrom(Expression sourceValue)
        {
            _sourceTypeHelper = new EnumerableTypeHelper(
                sourceValue.Type,
                ElementTypesAreTheSame ? _sourceElementType : sourceValue.Type.GetEnumerableElementType());

            _sourceVariable = GetSourceParameterFor(sourceValue.Type);
            var sourceVariableAssignment = Expression.Assign(_sourceVariable, sourceValue);

            _populationExpressions.Add(sourceVariableAssignment);
        }

        private ParameterExpression GetSourceParameterFor(Type type) => GetParameterFor(type, "source");

        private ParameterExpression GetTargetParameterFor(Type type) => GetParameterFor(type, "target");

        private ParameterExpression GetParameterFor(Type type, string sameTypesPrefix)
        {
            var parameterName = ElementTypesAreTheSame
                ? sameTypesPrefix + type.GetVariableNameInPascalCase()
                : type.GetVariableNameInCamelCase();

            var parameter = Expression.Parameter(type, parameterName);

            return parameter;
        }

        public void PopulateTargetVariableFromSourceObjectOnly()
            => AssignTargetVariableTo(GetSourceOnlyReturnValue());

        private Expression GetSourceOnlyReturnValue()
        {
            var convertedSourceItems = _sourceItemsSelector.SourceItemsProjectedToTargetType().GetResult();
            var returnValue = ConvertForReturnValue(convertedSourceItems);

            return returnValue;
        }

        private void AssignTargetVariableTo(Expression value)
        {
            TargetVariable = GetTargetParameterFor(value.Type);

            _populationExpressions.Add(Expression.Assign(TargetVariable, value));
        }

        public void AssignTargetVariable() => AssignTargetVariableTo(GetTargetVariableValue());

        private Expression GetTargetVariableValue()
        {
            if (_targetTypeHelper.IsArray && !_sourceTypeHelper.IsEnumerableInterface)
            {
                return GetCopyIntoWrapperConstruction(GetCountPropertyAccess());
            }

            Expression nonNullTargetVariableValue;

            if (_targetTypeHelper.IsArray || _targetTypeHelper.IsEnumerableInterface)
            {
                nonNullTargetVariableValue = GetNonNullEnumerableTargetVariableValue();
            }
            else if (_targetTypeHelper.HasCollectionInterface &&
                   !(_targetTypeHelper.IsList || _targetTypeHelper.IsCollection))
            {
                var isReadOnlyProperty = _targetTypeHelper
                    .CollectionInterfaceType
                    .GetPublicInstanceProperty("IsReadOnly");

                nonNullTargetVariableValue = Expression.Condition(
                    Expression.Property(_omd.TargetObject, isReadOnlyProperty),
                    GetCopyIntoListConstruction(),
                    _omd.TargetObject,
                    _omd.TargetObject.Type);
            }
            else
            {
                nonNullTargetVariableValue = _omd.TargetObject;
            }

            var nullTargetVariableType = nonNullTargetVariableValue.Type.IsInterface()
                ? _targetTypeHelper.ListType
                : nonNullTargetVariableValue.Type;

            var nullTargetVariableValue = _sourceTypeHelper.IsEnumerableInterface || _targetTypeHelper.IsCollection
                ? Expression.New(nullTargetVariableType)
                : Expression.New(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    nullTargetVariableType.GetConstructor(new[] { typeof(int) }),
                    GetCountPropertyAccess());

            var targetVariableValue = Expression.Condition(
                _omd.TargetObject.GetIsNotDefaultComparison(),
                nonNullTargetVariableValue,
                nullTargetVariableValue,
                nonNullTargetVariableValue.Type);

            return targetVariableValue;
        }

        private Expression GetCopyIntoWrapperConstruction(Expression numberOfNewItems)
        {
            var constructor = _targetTypeHelper.WrapperType
                .GetConstructor(new[] { _targetTypeHelper.EnumerableInterfaceType, typeof(int) });

            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(constructor, _omd.TargetObject, numberOfNewItems);
        }

        private Expression GetNonNullEnumerableTargetVariableValue()
        {
            var nonNullTargetVariableValue = GetCopyIntoListConstruction();

            if (_targetTypeHelper.IsArray)
            {
                return nonNullTargetVariableValue;
            }

            var targetIsCollection = Expression
                .TypeIs(_omd.TargetObject, _targetTypeHelper.CollectionInterfaceType);

            return Expression.Condition(
                targetIsCollection,
                _omd.TargetObject.GetConversionTo(_targetTypeHelper.CollectionInterfaceType),
                nonNullTargetVariableValue,
                _targetTypeHelper.CollectionInterfaceType);
        }

        private Expression GetCopyIntoListConstruction()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(
                _targetTypeHelper.ListType.GetConstructor(new[] { _targetTypeHelper.EnumerableInterfaceType }),
                _omd.TargetObject);
        }

        public void RemoveAllTargetItems()
        {
            _populationExpressions.Add(GetTargetMethodCall("Clear"));
        }

        public void AddNewItemsToTargetVariable(IObjectMappingData enumerableMappingData)
        {
            if (ElementTypesAreSimple && ElementTypesAreTheSame && _targetTypeHelper.IsList)
            {
                _populationExpressions.Add(GetTargetMethodCall("AddRange", _sourceVariable));
                return;
            }

            Func<LoopExpression, Expression> populationLoopAdapter;
            Expression loopExitCheck, sourceElement;

            if (_sourceTypeHelper.HasListInterface)
            {
                loopExitCheck = Expression.Equal(Counter, GetCountPropertyAccess());

                if (ElementTypesAreSimple ||
                    _sourceTypeHelper.ElementType.RuntimeTypeNeeded() ||
                    _targetElementType.RuntimeTypeNeeded())
                {
                    populationLoopAdapter = exp => exp;
                    sourceElement = GetIndexedElementAccess();
                }
                else
                {
                    sourceElement = GetSourceParameterFor(_sourceTypeHelper.ElementType);
                    populationLoopAdapter = loop => UpdateIndexAccessLoop(loop, (ParameterExpression)sourceElement);
                }
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

            var populationLoop = GetPopulationLoop(
                sourceElement,
                loopExitCheck,
                populationLoopAdapter,
                enumerableMappingData);

            var population = Expression.Block(
                new[] { Counter },
                Expression.Assign(Counter, Expression.Constant(0)),
                populationLoop);

            _populationExpressions.Add(population);
        }

        private Expression UpdateIndexAccessLoop(
            LoopExpression loop,
            ParameterExpression sourceElement)
        {
            var loopBody = (BlockExpression)loop.Body;
            var loopBodyExpressions = new List<Expression>(loopBody.Expressions);

            const int LOOP_EXIT_CHECK_INDEX = 0;
            var sourceElementAssignment = Expression.Assign(sourceElement, GetIndexedElementAccess());
            loopBodyExpressions.Insert(LOOP_EXIT_CHECK_INDEX + 1, sourceElementAssignment);

            loopBody = loopBody.Update(loopBody.Variables.Concat(sourceElement), loopBodyExpressions);

            return loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);
        }

        private Expression GetCountPropertyAccess()
        {
            if (_sourceTypeHelper.IsArray)
            {
                return Expression.Property(_sourceVariable, "Length");
            }

            var countPropertyInfo = _sourceTypeHelper.CollectionInterfaceType.GetPublicInstanceProperty("Count");

            return Expression.Property(_sourceVariable, countPropertyInfo);
        }

        private Expression GetIndexedElementAccess()
        {
            if (_sourceTypeHelper.IsArray)
            {
                return Expression.ArrayIndex(_sourceVariable, Counter);
            }

            var indexer = _sourceVariable.Type
                .GetPublicInstanceProperties()
                .First(p =>
                    (p.GetIndexParameters().Length == 1) &&
                    (p.GetIndexParameters()[0].ParameterType == typeof(int)));

            return Expression.MakeIndex(_sourceVariable, indexer, new[] { Counter });
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
            Func<LoopExpression, Expression> populationLoopAdapter,
            IObjectMappingData enumerableMappingData)
        {
            var breakLoop = Expression.Break(Expression.Label(typeof(void), "Break"));

            var elementToAdd = GetElementConversion(sourceElement, enumerableMappingData);
            var addMappedElement = GetTargetMethodCall("Add", elementToAdd);

            var loopBody = Expression.Block(
                Expression.IfThen(loopExitCheck, breakLoop),
                addMappedElement,
                Expression.PreIncrementAssign(Counter));

            var populationLoop = Expression.Loop(loopBody, breakLoop.Target);
            var adaptedLoop = populationLoopAdapter.Invoke(populationLoop);

            return adaptedLoop;
        }

        private Expression GetElementConversion(Expression sourceElement, IObjectMappingData enumerableMappingData)
        {
            return sourceElement.Type.IsSimple()
                ? GetSimpleElementConversion(sourceElement)
                : GetElementMapping(sourceElement, Expression.Default(_targetElementType), enumerableMappingData);
        }

        private Expression GetSimpleElementConversion(Expression sourceElement)
            => GetSimpleElementConversion(sourceElement, _targetElementType);

        private Expression GetSimpleElementConversion(Expression sourceElement, Type targetType)
            => _omd.MapperContext.ValueConverters.GetConversion(sourceElement, targetType);

        private static Expression GetElementMapping(
            Expression sourceElement,
            Expression targetElement,
            IObjectMappingData enumerableMappingData)
        {
            return MappingFactory.GetElementMapping(
                sourceElement,
                targetElement,
                enumerableMappingData);
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

        public void MapIntersection(IObjectMappingData enumerableMappingData)
        {
            var sourceElementParameter = GetSourceParameterFor(_sourceElementType);
            var targetElementParameter = GetTargetParameterFor(_targetElementType);

            var forEachActionType = Expression.GetActionType(_sourceElementType, _targetElementType, typeof(int));
            var forEachAction = GetElementMapping(sourceElementParameter, targetElementParameter, enumerableMappingData);

            var forEachLambda = Expression.Lambda(
                forEachActionType,
                forEachAction,
                sourceElementParameter,
                targetElementParameter,
                Counter);

            var forEachCall = Expression.Call(
                _forEachTupleMethod.MakeGenericMethod(_sourceElementType, _targetElementType),
                Expression.Property(_collectionDataVariable, "Intersection"),
                forEachLambda);

            _populationExpressions.Add(forEachCall);
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

            return value.WithToArrayCall(_targetElementType);
        }

        private Expression GetTargetMethodCall(string methodName, Expression argument = null)
        {
            var method = _targetTypeHelper.CollectionInterfaceType.GetMethod(methodName)
                ?? TargetVariable.Type.GetMethod(methodName);

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
            #region Untyped MethodInfos

            private static readonly MethodInfo _selectWithoutIndexMethod = typeof(Enumerable)
                    .GetPublicStaticMethods()
                    .Last(m => (m.Name == "Select") &&
                        (m.GetParameters().Length == 2) &&
                        (m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2));

            private static readonly MethodInfo _excludeMethod = typeof(EnumerableExtensions)
                .GetPublicStaticMethod("Exclude");

            #endregion

            private readonly EnumerablePopulationBuilder _builder;
            private Expression _result;

            internal SourceItemsSelector(EnumerablePopulationBuilder builder)
            {
                _builder = builder;
            }

            public SourceItemsSelector SourceItemsProjectedToTargetType()
            {
                if (_builder.ElementTypesAreTheSame)
                {
                    _result = _builder._omd.SourceObject;
                    return this;
                }

                var projectionFunc = Expression.Lambda(
                    Expression.GetFuncType(_builder._sourceElementType, _builder._targetElementType),
                    _builder.GetSimpleElementConversion(_builder._sourceElementParameter),
                    _builder._sourceElementParameter);

                var typedSelectMethod = _selectWithoutIndexMethod
                    .MakeGenericMethod(_builder._sourceElementType, _builder._targetElementType);

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