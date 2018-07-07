namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class EnumerablePopulationBuilder
    {
        #region Untyped MethodInfos

        public static readonly MethodInfo EnumerableSelectWithoutIndexMethod;
        private static readonly MethodInfo _enumerableSelectWithIndexMethod;
        private static readonly MethodInfo _queryableSelectMethod;
        private static readonly MethodInfo _forEachMethod;
        private static readonly MethodInfo _forEachTupleMethod;

        #endregion

        private readonly SourceItemsSelector _sourceItemsSelector;
        private readonly ISourceEnumerableAdapter _sourceAdapter;
        private ParameterExpression _sourceVariable;
        private readonly ParameterExpression _sourceElementParameter;
        private readonly ICollection<Expression> _populationExpressions;
        private LambdaExpression _sourceElementIdLambda;
        private LambdaExpression _targetElementIdLambda;
        private bool? _elementsAreIdentifiable;
        private ParameterExpression _collectionDataVariable;
        private ParameterExpression _counterVariable;
        private ParameterExpression _targetVariable;

        public EnumerablePopulationBuilder(ObjectMapperData mapperData)
        {
            MapperData = mapperData;
            Context = new EnumerablePopulationContext(mapperData);
            _sourceItemsSelector = new SourceItemsSelector(this);
            _sourceElementParameter = Context.SourceElementType.GetOrCreateParameter();
            TargetTypeHelper = new EnumerableTypeHelper(mapperData.TargetMember);

            _sourceAdapter = SourceEnumerableAdapterFactory.GetAdapterFor(this);
            _populationExpressions = new List<Expression>();
        }

        static EnumerablePopulationBuilder()
        {
            var linqSelectMethods = typeof(Enumerable)
                .GetPublicStaticMethods(nameof(Enumerable.Select))
                .Project(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .Filter(m => m.Parameters.Length == 2)
                .Project(m => new
                {
                    m.Method,
                    ProjectionLambdaParameterCount = m.Parameters[1].ParameterType.GetGenericTypeArguments().Length
                })
                .ToArray();

            EnumerableSelectWithoutIndexMethod = linqSelectMethods
                .First(m => m.ProjectionLambdaParameterCount == 2).Method;

            _enumerableSelectWithIndexMethod = linqSelectMethods
                .First(m => m.ProjectionLambdaParameterCount == 3).Method;

            _queryableSelectMethod = typeof(Queryable)
                .GetPublicStaticMethods(nameof(Enumerable.Select))
                .First(m =>
                    (m.GetParameters().Length == 2) &&
                    (m.GetParameters()[1].ParameterType.GetGenericTypeArguments()[0].GetGenericTypeArguments().Length == 2));

            var forEachMethods = typeof(EnumerableExtensions).GetPublicStaticMethods("ForEach").ToArray();
            _forEachMethod = forEachMethods.First();
            _forEachTupleMethod = forEachMethods.Last();
        }

        #region Operator

        public static implicit operator BlockExpression(EnumerablePopulationBuilder builder)
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

        public Expression GetCounterIncrement() => Expression.PreIncrementAssign(Counter);

        public ParameterExpression Counter => _counterVariable ?? (_counterVariable = GetCounterVariable());

        private ParameterExpression GetCounterVariable()
        {
            if (MapperData.IsRoot)
            {
                return Parameters.Create<int>("i");
            }

            var counterName = 'i';

            var parentMapperData = MapperData.Parent;

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

        #region Type Identification

        public bool ElementsAreIdentifiable
            => _elementsAreIdentifiable ?? (_elementsAreIdentifiable = DetermineIfElementsAreIdentifiable()).Value;

        private bool DetermineIfElementsAreIdentifiable()
        {
            if ((Context.SourceElementType == typeof(object)) ||
                (Context.TargetElementType == typeof(object)))
            {
                return false;
            }

            if ((Context.SourceElementType.IsValueType()) ||
                (Context.TargetElementType.IsValueType()))
            {
                return false;
            }

            var typeIdsCache = MapperData.MapperContext.Cache.CreateScoped<TypeKey, Expression>();
            var sourceElementId = GetIdentifierOrNull(Context.SourceElementType, _sourceElementParameter, MapperData, typeIdsCache);

            if (sourceElementId == null)
            {
                return false;
            }

            if (Context.ElementTypesAreTheSame)
            {
                _sourceElementIdLambda =
                    _targetElementIdLambda =
                        GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, sourceElementId);

                return true;
            }

            var targetElementParameter = Context.TargetElementType.GetOrCreateParameter();
            var targetElementId = GetIdentifierOrNull(Context.TargetElementType, targetElementParameter, MapperData, typeIdsCache);

            if (targetElementId == null)
            {
                return false;
            }

            _sourceElementIdLambda = GetSourceElementIdLambda(_sourceElementParameter, sourceElementId, targetElementId);
            _targetElementIdLambda = GetTargetElementIdLambda(targetElementParameter, targetElementId);

            return _targetElementIdLambda != null;
        }

        private static Expression GetIdentifierOrNull(
            Type type,
            Expression parameter,
            IMemberMapperData mapperData,
            ICache<TypeKey, Expression> cache)
        {
            return cache.GetOrAdd(TypeKey.ForTypeId(type), key =>
            {
                var configuredIdentifier =
                    mapperData.MapperContext.UserConfigurations.Identifiers.GetIdentifierOrNullFor(key.Type);

                if (configuredIdentifier != null)
                {
                    return configuredIdentifier.ReplaceParameterWith(parameter);
                }

                var identifier = mapperData
                    .MapperContext
                    .Naming
                    .GetIdentifierOrNull(key);

                return identifier?.GetAccess(parameter);
            });
        }

        private LambdaExpression GetSourceElementIdLambda(
            ParameterExpression sourceElement,
            Expression sourceElementId,
            Expression targetElementId)
        {
            return Expression.Lambda(
                Expression.GetFuncType(sourceElement.Type, targetElementId.Type),
                GetSimpleElementConversion(sourceElementId, targetElementId.Type),
                sourceElement);
        }

        private static LambdaExpression GetTargetElementIdLambda(ParameterExpression targetElement, Expression targetElementId)
        {
            return Expression.Lambda(
                Expression.GetFuncType(targetElement.Type, targetElementId.Type),
                targetElementId,
                targetElement);
        }

        #endregion

        public ObjectMapperData MapperData { get; }

        public EnumerablePopulationContext Context { get; }

        public bool ElementTypesAreSimple => Context.ElementTypesAreSimple;

        public EnumerableTypeHelper SourceTypeHelper { get; private set; }

        public Expression SourceValue { get; private set; }

        public Expression GetSourceCountAccess() => _sourceAdapter.GetSourceCountAccess();

        public Expression GetSourceIndexAccess() => SourceValue.GetIndexAccess(Counter);

        public EnumerableTypeHelper TargetTypeHelper { get; }

        public ParameterExpression TargetVariable
        {
            get => _targetVariable;
            set
            {
                if (_targetVariable == null)
                {
                    _targetVariable = value;
                }
            }
        }

        public void AssignSourceVariableFromSourceObject()
        {
            SourceValue = _sourceAdapter.GetSourceValues();

            if ((SourceValue == MapperData.SourceObject) && MapperData.HasSameSourceAsParent())
            {
                CreateSourceTypeHelper(SourceValue);
                return;
            }

            AssignSourceVariableFrom(SourceValue);

            var shortCircuit = _sourceAdapter.GetMappingShortCircuitOrNull();

            _populationExpressions.AddUnlessNullOrEmpty(shortCircuit);
        }

        public void AssignSourceVariableFrom(Func<SourceItemsSelector, SourceItemsSelector> sourceItemsSelection)
            => AssignSourceVariableFrom(sourceItemsSelection.Invoke(_sourceItemsSelector).GetResult());

        private void AssignSourceVariableFrom(Expression sourceValue)
        {
            CreateSourceTypeHelper(sourceValue);

            SourceValue = _sourceVariable = Context.GetSourceParameterFor(sourceValue.Type);

            _populationExpressions.Add(_sourceVariable.AssignTo(sourceValue));
        }

        private void CreateSourceTypeHelper(Expression sourceValue)
        {
            SourceTypeHelper = new EnumerableTypeHelper(
                sourceValue.Type,
                Context.ElementTypesAreTheSame ? Context.TargetElementType : sourceValue.Type.GetEnumerableElementType());
        }

        #region Target Variable Population

        public void PopulateTargetVariableFromSourceObjectOnly(IObjectMappingData mappingData = null)
            => AssignTargetVariableTo(GetSourceOnlyReturnValue(mappingData));

        private Expression GetSourceOnlyReturnValue(IObjectMappingData mappingData)
        {
            var convertedSourceItems = _sourceItemsSelector.SourceItemsProjectedToTargetType(mappingData).GetResult();
            var returnValue = ConvertForReturnValue(convertedSourceItems);

            return returnValue;
        }

        private void AssignTargetVariableTo(Expression value)
        {
            if (TargetVariable != null)
            {
                return;
            }

            TargetVariable = Context.GetTargetParameterFor(value.Type);

            _populationExpressions.Add(TargetVariable.AssignTo(value));
        }

        public void AssignTargetVariable()
        {
            AssignTargetVariableTo(GetTargetVariableValue());

            if (TargetCouldBeUnusable())
            {
                var targetVariableNull = TargetVariable.GetIsDefaultComparison();
                var returnExistingValue = Expression.Return(MapperData.ReturnLabelTarget, MapperData.TargetObject);
                var ifNullReturn = Expression.IfThen(targetVariableNull, returnExistingValue);

                _populationExpressions.Add(ifNullReturn);
            }
        }

        private Expression GetTargetVariableValue()
        {
            if (!MapperData.TargetMemberHasInitAccessibleValue())
            {
                return TargetTypeHelper.GetNewInstanceCreation();
            }

            if (MapperData.TargetIsDefinitelyUnpopulated())
            {
                return GetNullTargetListConstruction();
            }

            if (_sourceAdapter.UseReadOnlyTargetWrapper)
            {
                return GetCopyIntoWrapperConstruction();
            }

            Expression nonNullTargetVariableValue;

            if (TargetTypeHelper.IsDeclaredReadOnly)
            {
                nonNullTargetVariableValue = GetNonNullEnumerableTargetVariableValue();
            }
            else if (TargetTypeHelper.HasCollectionInterface &&
                   !(TargetTypeHelper.IsList || TargetTypeHelper.IsCollection))
            {
                var isReadOnlyProperty = TargetTypeHelper
                    .CollectionInterfaceType
                    .GetPublicInstanceProperty("IsReadOnly");

                nonNullTargetVariableValue = Expression.Condition(
                    Expression.Property(MapperData.TargetObject, isReadOnlyProperty),
                    GetUnusableTargetValue(MapperData.TargetObject.Type),
                    MapperData.TargetObject,
                    MapperData.TargetObject.Type);
            }
            else
            {
                nonNullTargetVariableValue = MapperData.TargetObject;
            }

            if (MapperData.TargetMember.IsReadOnly || MapperData.TargetIsDefinitelyPopulated())
            {
                return nonNullTargetVariableValue;
            }

            var nullTargetVariableValue = GetNullTargetListConstruction();

            var targetVariableValue = Expression.Condition(
                MapperData.TargetObject.GetIsNotDefaultComparison(),
                nonNullTargetVariableValue,
                nullTargetVariableValue,
                nonNullTargetVariableValue.Type);

            return targetVariableValue;
        }

        private Expression GetCopyIntoWrapperConstruction()
            => TargetTypeHelper.GetWrapperConstruction(MapperData.TargetObject, GetSourceCountAccess());

        private Expression GetNullTargetListConstruction()
        {
            var nonNullTargetVariableType =
               (TargetTypeHelper.IsDeclaredReadOnly ? TargetTypeHelper.ListType : MapperData.TargetType);

            var nullTargetVariableType = GetNullTargetVariableType(nonNullTargetVariableType);

            return SourceTypeHelper.IsEnumerableInterface || TargetTypeHelper.IsCollection
                ? Expression.New(nullTargetVariableType)
                : Expression.New(
                    nullTargetVariableType.GetPublicInstanceConstructor(typeof(int)),
                    GetSourceCountAccess());
        }

        private Expression GetNonNullEnumerableTargetVariableValue()
        {
            if (TargetTypeHelper.IsReadOnly)
            {
                return GetCopyIntoListConstruction();
            }

            var targetAsCollection = Expression
                .TypeAs(MapperData.TargetObject, TargetTypeHelper.CollectionInterfaceType);

            var tempCollection = Parameters.Create(targetAsCollection.Type, "collection");
            var assignedCollection = tempCollection.AssignTo(targetAsCollection);
            var assignedCollectionNotNull = assignedCollection.GetIsNotDefaultComparison();

            var unusableCollectionValue = GetUnusableTargetValue(tempCollection.Type);

            var isReadOnlyProperty = tempCollection.Type.GetPublicInstanceProperty("IsReadOnly");

            var writeableCollectionOrFallback = Expression.Condition(
                Expression.Property(tempCollection, isReadOnlyProperty),
                unusableCollectionValue,
                tempCollection,
                tempCollection.Type);

            var collectionResolution = Expression.Condition(
                assignedCollectionNotNull,
                writeableCollectionOrFallback,
                unusableCollectionValue,
                TargetTypeHelper.CollectionInterfaceType);

            return Expression.Block(new[] { tempCollection }, collectionResolution);
        }

        private Expression GetUnusableTargetValue(Type fallbackCollectionType)
        {
            return MapperData.TargetMember.IsReadOnly
                ? fallbackCollectionType.ToDefaultExpression()
                : GetCopyIntoListConstruction();
        }

        private Expression GetCopyIntoListConstruction()
        {
            return Expression.New(
                TargetTypeHelper.ListType.GetPublicInstanceConstructor(TargetTypeHelper.EnumerableInterfaceType),
                MapperData.TargetObject);
        }

        private Type GetNullTargetVariableType(Type nonNullTargetVariableType)
        {
            return nonNullTargetVariableType.IsInterface()
                ? TargetTypeHelper.ListType
                : nonNullTargetVariableType;
        }

        private bool TargetCouldBeUnusable()
        {
            if (MapperData.TargetMember.LeafMember.IsWriteable)
            {
                return false;
            }

            return !(TargetTypeHelper.IsList || TargetTypeHelper.IsCollection);
        }

        #endregion

        public void RemoveAllTargetItems()
        {
            _populationExpressions.Add(GetTargetMethodCall("Clear"));
        }

        public void AddNewItemsToTargetVariable(IObjectMappingData mappingData)
        {
            if (ElementTypesAreSimple && Context.ElementTypesAreTheSame && TargetTypeHelper.IsList)
            {
                _populationExpressions.Add(GetTargetMethodCall("AddRange", _sourceVariable));
                return;
            }

            BuildPopulationLoop((ld, md) => GetTargetMethodCall("Add", ld.GetElementMapping(md)), mappingData);
        }

        public void BuildPopulationLoop(
            Func<IPopulationLoopData, IObjectMappingData, Expression> elementPopulationFactory,
            IObjectMappingData mappingData)
        {
            var loopData = _sourceAdapter.GetPopulationLoopData();

            var populationLoop = loopData.BuildPopulationLoop(
                this,
                mappingData,
                elementPopulationFactory);

            _populationExpressions.Add(populationLoop);
        }

        public Expression GetElementConversion(Expression sourceElement, IObjectMappingData mappingData)
        {
            if (ElementTypesAreSimple)
            {
                return GetSimpleElementConversion(sourceElement);
            }

            var targetMember = mappingData.MapperData.TargetMember;

            Expression existingElementValue;

            if (targetMember.CheckExistingElementValue)
            {
                var existingElementValueCheck = targetMember.GetAccessChecked(mappingData.MapperData);

                if (existingElementValueCheck.Variables.Any())
                {
                    return GetValueCheckedElementMapping(sourceElement, existingElementValueCheck, mappingData);
                }

                existingElementValue = existingElementValueCheck;
            }
            else
            {
                existingElementValue = Context.TargetElementType.ToDefaultExpression();
            }

            return GetElementMapping(sourceElement, existingElementValue, mappingData);
        }

        private static Expression GetValueCheckedElementMapping(
            Expression sourceElement,
            BlockExpression existingElementValueCheck,
            IObjectMappingData mappingData)
        {
            var valueVariable = existingElementValueCheck.Variables[0];
            var mapping = GetElementMapping(sourceElement, valueVariable, mappingData);

            if (mapping.NodeType != ExpressionType.Try)
            {
                return Expression.Block(
                    new[] { valueVariable },
                    existingElementValueCheck.Expressions.Append(mapping));
            }

            var mappingTryCatch = (TryExpression)mapping;

            mapping = mappingTryCatch.Update(
                Expression.Block(existingElementValueCheck.Expressions.Append(mappingTryCatch.Body)),
                mappingTryCatch.Handlers,
                mappingTryCatch.Finally,
                mappingTryCatch.Fault);

            return Expression.Block(new[] { valueVariable }, mapping);
        }

        public Expression GetSimpleElementConversion(Expression sourceElement)
            => GetSimpleElementConversion(sourceElement, Context.TargetElementType);

        private Expression GetSimpleElementConversion(Expression sourceElement, Type targetType)
            => MapperData.GetValueConversion(sourceElement, targetType);

        private static Expression GetElementMapping(
            Expression sourceElement,
            Expression targetElement,
            IObjectMappingData mappingData)
        {
            return MappingFactory.GetElementMapping(sourceElement, targetElement, mappingData);
        }

        public Expression GetSourceItemsProjection(
            Expression sourceEnumerableValue,
            Func<Expression, Expression> projectionLambdaFactory)
        {
            return CreateSourceItemsProjection(
                sourceEnumerableValue,
                (sourceParameter, counter) => projectionLambdaFactory.Invoke(sourceParameter),
                counterRequired: false);
        }

        public Expression GetSourceItemsProjection(
            Expression sourceEnumerableValue,
            Func<Expression, Expression, Expression> projectionLambdaFactory)
        {
            return CreateSourceItemsProjection(sourceEnumerableValue, projectionLambdaFactory, counterRequired: true);
        }

        private Expression CreateSourceItemsProjection(
            Expression sourceEnumerableValue,
            Func<Expression, Expression, Expression> projectionLambdaFactory,
            bool counterRequired)
        {
            var isRootQueryableMapping = MapperData.SourceType.IsQueryable();

            if (isRootQueryableMapping || MapperData.Context.IsPartOfQueryableMapping())
            {
                counterRequired = false;
            }

            var linqSelectOverload = isRootQueryableMapping
                ? _queryableSelectMethod
                : counterRequired
                    ? _enumerableSelectWithIndexMethod
                    : EnumerableSelectWithoutIndexMethod;

            ParameterExpression[] projectionLambdaParameters;
            Type[] funcTypes;

            if (counterRequired)
            {
                projectionLambdaParameters = new[] { _sourceElementParameter, Counter };
                funcTypes = new[] { Context.SourceElementType, Counter.Type, Context.TargetElementType };
            }
            else
            {
                projectionLambdaParameters = new[] { _sourceElementParameter };
                funcTypes = new[] { Context.SourceElementType, Context.TargetElementType };
            }

            var projectionFuncType = Expression.GetFuncType(funcTypes);

            Expression projectionLambda = Expression.Lambda(
                projectionFuncType,
                projectionLambdaFactory.Invoke(_sourceElementParameter, Counter),
                projectionLambdaParameters);

            var typedSelectMethod = linqSelectOverload.MakeGenericMethod(Context.ElementTypes);
            var typedSelectCall = Expression.Call(typedSelectMethod, sourceEnumerableValue, projectionLambda);

            return typedSelectCall;
        }

        public void CreateCollectionData()
        {
            var createCollectionDataMethod = Context.ElementTypesAreTheSame
                ? CollectionData.IdSameTypesCreateMethod
                    .MakeGenericMethod(Context.TargetElementType, _targetElementIdLambda.ReturnType)
                : CollectionData.IdDifferentTypesCreateMethod
                    .MakeGenericMethod(Context.SourceElementType, Context.TargetElementType, _targetElementIdLambda.ReturnType);

            var callArguments = new List<Expression>(4)
            {
                _sourceAdapter.GetSourceValues(),
                MapperData.TargetObject,
                _sourceElementIdLambda
            };

            if (!Context.ElementTypesAreTheSame)
            {
                callArguments.Add(_targetElementIdLambda);
            }

            var createCollectionDataCall = Expression.Call(createCollectionDataMethod, callArguments);

            _collectionDataVariable = Parameters.Create(
                typeof(CollectionData<,>).MakeGenericType(Context.ElementTypes),
                "collectionData");

            var assignCollectionData = _collectionDataVariable.AssignTo(createCollectionDataCall);

            _populationExpressions.Add(assignCollectionData);
        }

        public void MapIntersection(IObjectMappingData enumerableMappingData)
        {
            var sourceElementParameter = Context.GetSourceParameterFor(Context.SourceElementType);
            var targetElementParameter = Context.GetTargetParameterFor(Context.TargetElementType);

            var forEachActionType = Expression.GetActionType(Context.SourceElementType, Context.TargetElementType, typeof(int));
            var forEachAction = GetElementMapping(sourceElementParameter, targetElementParameter, enumerableMappingData);

            var forEachLambda = Expression.Lambda(
                forEachActionType,
                forEachAction,
                sourceElementParameter,
                targetElementParameter,
                Counter);

            var forEachCall = Expression.Call(
                _forEachTupleMethod.MakeGenericMethod(Context.ElementTypes),
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

        public Expression GetReturnValue() => ConvertForReturnValue(TargetVariable);

        private Expression ConvertForReturnValue(Expression value)
        {
            var allowSameValue = value.NodeType != ExpressionType.MemberAccess;

            if (!allowSameValue)
            {
                return GetEnumerableConversion(value);
            }

            if (value.Type.IsAssignableTo(MapperData.TargetType))
            {
                return value;
            }

            var conversion = GetEnumerableConversion(value);

            if (MapperData.TargetType.IsInterface())
            {
                conversion = Expression.Coalesce(Expression.TypeAs(value, MapperData.TargetType), conversion);
            }

            return conversion;

        }

        public Expression GetEnumerableConversion(Expression value)
            => TargetTypeHelper.GetEnumerableConversion(value, MapperData.RuleSet.Settings.AllowEnumerableAssignment);

        private Expression GetTargetMethodCall(string methodName, Expression argument = null)
        {
            var method = TargetTypeHelper.CollectionInterfaceType.GetPublicInstanceMethod(methodName)
                ?? TargetVariable.Type.GetPublicInstanceMethod(methodName);

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

            public SourceItemsSelector SourceItemsProjectedToTargetType(IObjectMappingData mappingData = null)
            {
                var context = _builder.Context;
                var sourceEnumerableValue = _builder._sourceAdapter.GetSourceValues();

                if (context.ElementTypesAreTheSame ||
                   (sourceEnumerableValue.Type.GetEnumerableElementType() == context.TargetElementType))
                {
                    _result = sourceEnumerableValue;
                    return this;
                }

                _result = _builder.GetSourceItemsProjection(
                    sourceEnumerableValue,
                    (sourceElement, counter) => _builder.GetElementConversion(sourceElement, mappingData));

                return this;
            }

            public SourceItemsSelector ExcludingTargetItems()
            {
                var excludeMethod = typeof(EnumerableExtensions)
                    .GetPublicStaticMethod("Exclude")
                    .MakeGenericMethod(_builder.Context.TargetElementType);

                _result = Expression.Call(
                    excludeMethod,
                    _result,
                    _builder.MapperData.TargetObject);

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

                _result = _builder.GetEnumerableConversion(_result);

                return _result;
            }
        }
    }
}