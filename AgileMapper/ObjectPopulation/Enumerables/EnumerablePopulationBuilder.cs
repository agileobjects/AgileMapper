namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
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

        private static readonly MethodInfo _selectWithoutIndexMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .Last(m => (m.Name == "Select") &&
                (m.GetParameters().Length == 2) &&
                (m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2));

        private static readonly MethodInfo _selectWithIndexMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .Last(m => (m.Name == "Select") &&
                (m.GetParameters().Length == 2) &&
                (m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 3));

        private static readonly MethodInfo _forEachMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods()
            .First(m => m.Name == "ForEach");

        private static readonly MethodInfo _forEachTupleMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods()
            .Last(m => m.Name == "ForEach");

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

        public EnumerablePopulationBuilder(ObjectMapperData mapperData)
        {
            MapperData = mapperData;
            Context = new EnumerablePopulationContext(mapperData);
            _sourceItemsSelector = new SourceItemsSelector(this);
            _sourceElementParameter = Context.SourceElementType.GetOrCreateParameter();
            TargetTypeHelper = new EnumerableTypeHelper(mapperData.TargetType, mapperData.TargetMember.ElementType);

            _sourceAdapter = SourceEnumerableAdapterFactory.GetAdapterFor(this);
            _populationExpressions = new List<Expression>();
        }

        #region Operator

        public static implicit operator BlockExpression(EnumerablePopulationBuilder builder)
        {
            if (builder._populationExpressions.None())
            {
                return null;
            }

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

                var identifier = GlobalContext.Instance.MemberFinder.GetIdentifierOrNull(key);

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

        public ParameterExpression TargetVariable { get; private set; }

        public void AssignSourceVariableFromSourceObject()
        {
            SourceValue = _sourceAdapter.GetSourceValue();

            if ((SourceValue == MapperData.SourceObject) && MapperData.HasSameSourceAsParent())
            {
                CreateSourceTypeHelper(SourceValue);
                return;
            }

            AssignSourceVariableFrom(SourceValue);
        }

        public void AssignSourceVariableFrom(Func<SourceItemsSelector, SourceItemsSelector> sourceItemsSelection)
            => AssignSourceVariableFrom(sourceItemsSelection.Invoke(_sourceItemsSelector).GetResult());

        private void AssignSourceVariableFrom(Expression sourceValue)
        {
            CreateSourceTypeHelper(sourceValue);

            _sourceVariable = Context.GetSourceParameterFor(sourceValue.Type);
            var sourceVariableAssignment = _sourceVariable.AssignTo(sourceValue);

            SourceValue = _sourceVariable;

            _populationExpressions.Add(sourceVariableAssignment);
        }

        private void CreateSourceTypeHelper(Expression sourceValue)
        {
            SourceTypeHelper = new EnumerableTypeHelper(
                sourceValue.Type,
                Context.ElementTypesAreTheSame ? Context.TargetElementType : sourceValue.Type.GetEnumerableElementType());
        }

        #region Target Variable Population

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

            if (MapperData.TargetMember.IsReadOnly)
            {
                return nonNullTargetVariableValue;
            }

            var nullTargetVariableType = nonNullTargetVariableValue.Type.IsInterface()
                ? TargetTypeHelper.ListType
                : nonNullTargetVariableValue.Type;

            var nullTargetVariableValue = SourceTypeHelper.IsEnumerableInterface || TargetTypeHelper.IsCollection
                ? Expression.New(nullTargetVariableType)
                : Expression.New(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    nullTargetVariableType.GetConstructor(new[] { typeof(int) }),
                    GetSourceCountAccess());

            var targetVariableValue = Expression.Condition(
                MapperData.TargetObject.GetIsNotDefaultComparison(),
                nonNullTargetVariableValue,
                nullTargetVariableValue,
                nonNullTargetVariableValue.Type);

            return targetVariableValue;
        }

        private Expression GetCopyIntoWrapperConstruction()
            => TargetTypeHelper.GetWrapperConstruction(MapperData.TargetObject, GetSourceCountAccess());

        private Expression GetNonNullEnumerableTargetVariableValue()
        {
            if (TargetTypeHelper.IsReadOnly)
            {
                return GetCopyIntoListConstruction();
            }

            var targetIsCollection = Expression
                .TypeIs(MapperData.TargetObject, TargetTypeHelper.CollectionInterfaceType);

            var collectionValue = MapperData.TargetObject.GetConversionTo(TargetTypeHelper.CollectionInterfaceType);
            var nonCollectionValue = GetUnusableTargetValue(collectionValue.Type);

            return Expression.Condition(
                targetIsCollection,
                collectionValue,
                nonCollectionValue,
                TargetTypeHelper.CollectionInterfaceType);
        }

        private Expression GetUnusableTargetValue(Type fallbackCollectionType)
        {
            return MapperData.TargetMember.IsReadOnly
                ? fallbackCollectionType.ToDefaultExpression()
                : GetCopyIntoListConstruction();
        }

        private Expression GetCopyIntoListConstruction()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(
                TargetTypeHelper.ListType.GetConstructor(new[] { TargetTypeHelper.EnumerableInterfaceType }),
                MapperData.TargetObject);
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
                _populationExpressions.Add(GetTargetMethodCall("AddRange", _sourceAdapter.GetSourceValues()));
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
            Func<Expression, Expression> projectionFuncFactory)
        {
            return GetSourceItemsProjection(
                sourceEnumerableValue,
                _selectWithoutIndexMethod,
                (sourceParameter, counter) => projectionFuncFactory.Invoke(sourceParameter),
                _sourceElementParameter);
        }

        public Expression GetSourceItemsProjection(
            Expression sourceEnumerableValue,
            Func<Expression, Expression, Expression> projectionFuncFactory)
        {
            return GetSourceItemsProjection(
                sourceEnumerableValue,
                _selectWithIndexMethod,
                projectionFuncFactory,
                _sourceElementParameter,
                Counter);
        }

        private Expression GetSourceItemsProjection(
            Expression sourceEnumerableValue,
            MethodInfo linqSelectOverload,
            Func<Expression, Expression, Expression> projectionFuncFactory,
            params ParameterExpression[] projectionFuncParameters)
        {
            var funcTypes = projectionFuncParameters
                .Select(p => p.Type)
                .ToArray()
                .Append(Context.TargetElementType);

            var projectionFunc = Expression.Lambda(
                Expression.GetFuncType(funcTypes),
                projectionFuncFactory.Invoke(_sourceElementParameter, Counter),
                projectionFuncParameters);

            var typedSelectMethod = linqSelectOverload.MakeGenericMethod(Context.ElementTypes);
            var typedSelectCall = Expression.Call(typedSelectMethod, sourceEnumerableValue, projectionFunc);

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
                _sourceAdapter.GetSourceValue(),
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

            if (allowSameValue && MapperData.TargetType.IsAssignableFrom(value.Type))
            {
                return value;
            }

            return TargetTypeHelper.GetEnumerableConversion(value);
        }

        private Expression GetTargetMethodCall(string methodName, Expression argument = null)
        {
            var method = TargetTypeHelper.CollectionInterfaceType.GetMethod(methodName)
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
                var context = _builder.Context;
                var sourceEnumerableValue = _builder._sourceAdapter.GetSourceValue();

                if (context.ElementTypesAreTheSame ||
                    (sourceEnumerableValue.Type.GetEnumerableElementType() == context.TargetElementType))
                {
                    _result = sourceEnumerableValue;
                    return this;
                }

                _result = _builder.GetSourceItemsProjection(
                    sourceEnumerableValue,
                    _builder.GetSimpleElementConversion);

                return this;
            }

            public SourceItemsSelector ExcludingTargetItems()
            {
                _result = Expression.Call(
                    _excludeMethod.MakeGenericMethod(_builder.Context.TargetElementType),
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

                _result = _builder.TargetTypeHelper.GetEnumerableConversion(_result);

                return _result;
            }
        }
    }
}