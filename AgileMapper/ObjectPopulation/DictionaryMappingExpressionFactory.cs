namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Enumerables;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions;

    internal class DictionaryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly MemberPopulationFactory _memberPopulationFactory;

        public DictionaryMappingExpressionFactory()
        {
            _memberPopulationFactory = new MemberPopulationFactory(GetDataSources);
        }

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsDictionary;

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperKey.MappingTypes.SourceType.IsDictionary())
            {
                nullMappingBlock = null;
                return false;
            }

            var targetMember = (DictionaryTargetMember)mappingData.MapperData.TargetMember;

            if ((targetMember.KeyType == typeof(string)) || (targetMember.KeyType == typeof(object)))
            {
                nullMappingBlock = null;
                return false;
            }

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("Only string- or object-keyed Dictionaries are supported"),
                mappingData.MapperData.GetFallbackCollectionValue());

            return true;
        }

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
            => Enumerable<Expression>.Empty;

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsDictionary)
            {
                DictionarySourceMember sourceDictionaryMember;

                if (SourceMemberIsDictionary(mapperData, out sourceDictionaryMember))
                {
                    if (UseDictionaryCloneConstructor(sourceDictionaryMember, mapperData))
                    {
                        yield return GetClonedDictionaryAssignment(mapperData);
                        yield break;
                    }

                    yield return GetMappedDictionaryAssignment(sourceDictionaryMember, mappingData);
                    yield return GetDictionaryToDictionaryMapping(sourceDictionaryMember, mappingData);
                    yield break;
                }

                yield return GetParameterlessDictionaryAssignment(mappingData);
            }

            yield return GetDictionaryPopulation(mappingData);
        }

        private static bool SourceMemberIsDictionary(
            IMemberMapperData mapperData,
            out DictionarySourceMember sourceDictionaryMember)
        {
            sourceDictionaryMember = mapperData.GetDictionarySourceMemberOrNull();
            return sourceDictionaryMember != null;
        }

        private static bool UseDictionaryCloneConstructor(
            IQualifiedMember sourceDictionaryMember,
            IBasicMapperData mapperData)
        {
            return mapperData.TargetMember.ElementType.IsSimple() &&
                  (sourceDictionaryMember.Type == mapperData.TargetType);
        }

        private static Expression GetClonedDictionaryAssignment(IMemberMapperData mapperData)
        {
            var cloneConstructor = GetDictionaryCloneConstructor(mapperData.TargetMember.Type);
            var comparer = Expression.Property(mapperData.SourceObject, "Comparer");
            var cloneDictionary = Expression.New(cloneConstructor, mapperData.SourceObject, comparer);
            var assignment = mapperData.InstanceVariable.AssignTo(cloneDictionary);

            return assignment;
        }

        private static ConstructorInfo GetDictionaryCloneConstructor(Type dictionaryType)
        {
            var dictionaryTypes = dictionaryType.GetGenericArguments();
            var dictionaryInterfaceType = typeof(IDictionary<,>).MakeGenericType(dictionaryTypes);

            return FindDictionaryConstructor(dictionaryType, dictionaryInterfaceType, numberOfParameters: 2);
        }

        private static ConstructorInfo FindDictionaryConstructor(
            Type dictionaryType,
            Type firstParameterType,
            int numberOfParameters)
        {
            return dictionaryType
                .GetPublicInstanceConstructors()
                .Select(ctor => new { Ctor = ctor, Parameters = ctor.GetParameters() })
                .First(ctor =>
                    (ctor.Parameters.Length == numberOfParameters) &&
                    (ctor.Parameters[0].ParameterType == firstParameterType))
                .Ctor;
        }

        private static Expression GetMappedDictionaryAssignment(
            DictionarySourceMember sourceDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (UseParameterlessConstructor(sourceDictionaryMember, mapperData))
            {
                return GetParameterlessDictionaryAssignment(mappingData);
            }

            var comparer = Expression.Property(mapperData.SourceObject, "Comparer");

            var constructor = FindDictionaryConstructor(
                mapperData.TargetType,
                comparer.Type,
                numberOfParameters: 1);

            return GetDictionaryAssignment(Expression.New(constructor, comparer), mappingData);
        }

        private static bool UseParameterlessConstructor(
            DictionarySourceMember sourceDictionaryMember,
            IBasicMapperData mapperData)
        {
            if (sourceDictionaryMember.Type.IsInterface())
            {
                return true;
            }

            return sourceDictionaryMember.ValueType != mapperData.TargetMember.ElementType;
        }

        private static Expression GetDictionaryToDictionaryMapping(
            DictionarySourceMember sourceDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var keyValuePairType = typeof(KeyValuePair<,>)
                .MakeGenericType(sourceDictionaryMember.KeyType, sourceDictionaryMember.ValueType);

            var loopData = new EnumerableSourcePopulationLoopData(
                mapperData.EnumerablePopulationBuilder,
                keyValuePairType,
                mapperData.SourceObject);

            var populationLoop = loopData.BuildPopulationLoop(
                mapperData.EnumerablePopulationBuilder,
                mappingData,
                GetSourceDictionaryTargetEntryAssignment);

            return populationLoop;
        }

        private static Expression GetSourceDictionaryTargetEntryAssignment(
            EnumerableSourcePopulationLoopData loopData,
            IObjectMappingData mappingData)
        {
            return GetTargetEntryAssignment(
                loopData,
                mappingData,
                eld => Expression.Property(eld.SourceElement, "Key"),
                eld => Expression.Property(eld.SourceElement, "Value"));
        }

        private static Expression GetTargetEntryAssignment(
            EnumerableSourcePopulationLoopData loopData,
            IObjectMappingData mappingData,
            Func<EnumerableSourcePopulationLoopData, Expression> targetElementKeyFactory,
            Func<EnumerableSourcePopulationLoopData, Expression> targetElementValueFactory)
        {
            var mapperData = loopData.Builder.MapperData;
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var keyVariable = Expression.Variable(targetDictionaryMember.KeyType, "targetKey");
            var keyAccess = targetElementKeyFactory.Invoke(loopData);
            var keyConversion = mapperData.GetValueConversion(keyAccess, keyVariable.Type);
            var keyAssignment = keyVariable.AssignTo(keyConversion);

            var valueAccess = targetElementValueFactory.Invoke(loopData);
            var valueConversion = GetElementConversion(loopData, valueAccess, mappingData);

            var targetDictionaryEntryMember = targetDictionaryMember.Append(keyVariable);
            var targetEntryAssignment = targetDictionaryEntryMember.GetPopulation(valueConversion, mapperData);

            var childMapperData = new ChildMemberMapperData(targetDictionaryEntryMember, mappingData.MapperData);
            var childMappingData = mappingData.GetChildMappingData(childMapperData);

            var populationGuard = childMappingData.GetRuleSetPopulationGuardOrNull();

            if (populationGuard != null)
            {
                targetEntryAssignment = Expression.IfThen(populationGuard, targetEntryAssignment);
            }

            return Expression.Block(new[] { keyVariable }, keyAssignment, targetEntryAssignment);
        }

        private static Expression GetElementConversion(
            EnumerableSourcePopulationLoopData loopData,
            Expression value,
            IObjectMappingData mappingData)
        {
            if (value.Type.IsSimple())
            {
                var targetType = ((DictionaryTargetMember)mappingData.MapperData.TargetMember).ValueType;

                return mappingData.MapperData.GetValueConversion(value, targetType);
            }

            return loopData.Builder.GetElementConversion(value, mappingData);
        }

        private static Expression GetParameterlessDictionaryAssignment(IObjectMappingData mappingData)
        {
            var newDictionary = mappingData.MapperData.TargetType.GetEmptyInstanceCreation();

            return GetDictionaryAssignment(newDictionary, mappingData);
        }

        private static Expression GetDictionaryAssignment(Expression value, IObjectMappingData mappingData)
        {
            value = AddExistingTargetCheckIfAppropriate(value, mappingData);

            return mappingData.MapperData.InstanceVariable.AssignTo(value);
        }

        private Expression GetDictionaryPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            if (mapperData.TargetMember.IsEnumerable && mapperData.TargetMemberIsEnumerableElement())
            {
                if (targetDictionaryMember.ValueType.IsEnumerable())
                {
                    return GetEnumerableToEnumerableMapping(mappingData);
                }

                return GetEnumerableToDictionaryPopulationLoop(mappingData);
            }

            var allTargetMemberMapperDataPairs = EnumerateTargetMembers(
                mapperData.SourceType,
                targetDictionaryMember,
                mappingData);

            var memberPopulations = allTargetMemberMapperDataPairs
                .Select(pair => GetMemberPopulation(pair.Item1, pair.Item2))
                .ToArray();

            if (memberPopulations.HasOne())
            {
                return memberPopulations[0];
            }

            var memberPopulationBlock = Expression.Block(memberPopulations);

            return memberPopulationBlock;
        }

        private static Expression GetEnumerableToEnumerableMapping(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var dictionaryEntryMember = (DictionaryTargetMember)mapperData.TargetMember;
            var dictionaryEntryVariable = Expression.Variable(dictionaryEntryMember.ValueType, "enumerable");
            var tryGetValueCall = dictionaryEntryMember.GetTryGetValueCall(dictionaryEntryVariable, mapperData);

            var mappingValues = new MappingValues(
                mapperData.SourceObject,
                dictionaryEntryVariable,
                mapperData.EnumerableIndex);

            var enumerableMapping = MappingFactory.GetChildMapping(
                mappingData,
                mappingValues,
                0,
                mapperData);

            var dictionaryEntryAssignment = dictionaryEntryMember.GetPopulation(enumerableMapping, mapperData);

            return Expression.Block(
                new[] { dictionaryEntryVariable },
                tryGetValueCall,
                dictionaryEntryAssignment);
        }

        private static IEnumerable<Tuple<DictionaryTargetMember, IObjectMappingData>> EnumerateTargetMembers(
            Type parentSourceType,
            DictionaryTargetMember targetDictionaryMember,
            IObjectMappingData mappingData)
        {
            if (parentSourceType.IsEnumerable())
            {
                yield return Tuple.Create(targetDictionaryMember, mappingData);
                yield break;
            }

            var sourceMembers = GlobalContext.Instance.MemberFinder.GetSourceMembers(parentSourceType);

            foreach (var sourceMember in sourceMembers)
            {
                var entryTargetMember = targetDictionaryMember.Append(sourceMember.Name);

                if (sourceMember.IsSimple)
                {
                    yield return Tuple.Create(entryTargetMember, mappingData);
                    continue;
                }

                var childMappingData = GetChildMappingData(sourceMember, entryTargetMember, mappingData);

                var childMemberMapperDataPairs = EnumerateTargetMembers(
                    sourceMember.Type,
                    entryTargetMember,
                    childMappingData);

                foreach (var memberMappingDataPair in childMemberMapperDataPairs)
                {
                    yield return memberMappingDataPair;
                }
            }
        }

        private static IObjectMappingData GetChildMappingData(
            Member nonSimpleSourceMember,
            DictionaryTargetMember entryTargetMember,
            IObjectMappingData mappingData)
        {
            if (nonSimpleSourceMember.IsComplex)
            {
                return mappingData;
            }

            var qualifiedSourceMember = mappingData.MapperData.SourceMember.Append(nonSimpleSourceMember);
            entryTargetMember = (DictionaryTargetMember)entryTargetMember.WithType(qualifiedSourceMember.Type);

            var childMappingData = ObjectMappingDataFactory.ForChild(
                qualifiedSourceMember,
                entryTargetMember,
                0,
                mappingData);

            return childMappingData;
        }

        private Expression GetMemberPopulation(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var memberPopulation = _memberPopulationFactory.Create(targetMember, mappingData);
            var populationExpression = memberPopulation.GetPopulation();

            return populationExpression;
        }

        private DataSourceSet GetDataSources(IChildMemberMappingData childMappingData)
        {
            if (childMappingData.MapperData.SourceMember.IsEnumerable)
            {
                var mappingDataSource = GetEnumerableToDictionaryDataSource(childMappingData.Parent);

                return new DataSourceSet(mappingDataSource);
            }

            return DataSourceFinder.FindDataSources(childMappingData);
        }

        private IDataSource GetEnumerableToDictionaryDataSource(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var contextMapperData = mapperData.IsRoot ? mapperData : mapperData.Parent;
            var sourceMemberDataSource = SourceMemberDataSource.For(mapperData.SourceMember, contextMapperData);
            var populationLoop = GetEnumerableToDictionaryPopulationLoop(mappingData);

            if (mapperData.Context.IsStandalone)
            {
                return new AdHocDataSource(sourceMemberDataSource, populationLoop);
            }

            var mappingValues = new MappingValues(
                sourceMemberDataSource.Value,
                mapperData.TargetType.ToDefaultExpression(),
                mapperData.Parent.EnumerableIndex);

            var directAccessPopulationLoop = MappingFactory.GetDirectAccessMapping(
                populationLoop,
                mapperData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, 0, mapperData));

            return new AdHocDataSource(sourceMemberDataSource, directAccessPopulationLoop);
        }

        private Expression GetEnumerableToDictionaryPopulationLoop(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var loopData = new EnumerableSourcePopulationLoopData(
                mapperData.EnumerablePopulationBuilder,
                mapperData.EnumerablePopulationBuilder.Context.SourceElementType,
                mapperData.SourceObject);

            var populationLoop = loopData.BuildPopulationLoop(
                mapperData.EnumerablePopulationBuilder,
                mappingData,
                GetSourceEnumerableTargetEntryAssignment);

            return populationLoop;
        }

        private Expression GetSourceEnumerableTargetEntryAssignment(
            EnumerableSourcePopulationLoopData loopData,
            IObjectMappingData enumerableMappingData)
        {
            if (loopData.Builder.Context.SourceElementType.IsSimple())
            {
                return GetTargetEntryAssignment(
                    loopData,
                    enumerableMappingData,
                    eld =>
                    {
                        var targetElementMember = eld.Builder.MapperData.TargetMember.GetElementMember();
                        var targetElementMapperData = new ChildMemberMapperData(targetElementMember, eld.Builder.MapperData);
                        var targetElementKey = targetElementMapperData.GetTargetMemberDictionaryKey();

                        return targetElementKey;
                    },
                    eld => eld.SourceElement);
            }

            var elementMappingData = CreateElementMappingData(enumerableMappingData);
            var elementMapping = GetDictionaryPopulation(elementMappingData);
            var elementMapperData = elementMappingData.MapperData;

            var mappingValues = new MappingValues(
                loopData.SourceElement,
                elementMapperData.TargetType.ToDefaultExpression(),
                loopData.Builder.Counter);

            var directMapping = MappingFactory.GetDirectAccessMapping(
                elementMapping,
                elementMapperData,
                mappingValues,
                MappingDataCreationFactory.ForElement(mappingValues, elementMapperData));

            return directMapping;
        }

        private static IObjectMappingData CreateElementMappingData(IObjectMappingData enumerableMappingData)
        {
            var builder = enumerableMappingData.MapperData.EnumerablePopulationBuilder;
            var targetElementType = GetTargetElementType(builder);

            var elementMappingData = ObjectMappingDataFactory.ForElement(
                builder.Context.SourceElementType,
                targetElementType,
                enumerableMappingData);

            return elementMappingData;
        }

        private static Type GetTargetElementType(EnumerablePopulationBuilder builder)
        {
            if (builder.Context.ElementTypesAreSimple ||
               (builder.Context.TargetElementType == typeof(object)))
            {
                return builder.Context.SourceElementType;
            }

            return builder.Context.TargetElementType;
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;
    }
}