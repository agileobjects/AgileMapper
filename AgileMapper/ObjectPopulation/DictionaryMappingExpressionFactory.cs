namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Enumerables.Dictionaries;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions;

    internal class DictionaryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly MemberPopulationFactory _memberPopulationFactory;

        public DictionaryMappingExpressionFactory()
        {
            _memberPopulationFactory = new MemberPopulationFactory(GetAllTargetMembers);
        }

        private static IEnumerable<QualifiedMember> GetAllTargetMembers(IMemberMapperData mapperData)
        {
            var allTargetMembers = EnumerateTargetMembers(mapperData).ToArray();

            var configuredDataSourceFactories = mapperData.MapperContext
                .UserConfigurations
                .DataSourceFactories
                .OfType<ConfiguredDictionaryDataSourceFactory>()
                .Where(dsf => dsf.IsFor(mapperData))
                .ToArray();

            if (configuredDataSourceFactories.Any())
            {
                var configuredCustomTargetMembers = configuredDataSourceFactories
                    .Where(dsf => allTargetMembers.None(dsf.Matches))
                    .GroupBy(dsf => dsf.TargetDictionaryEntryMember.Name)
                    .Select(group => group.First().TargetDictionaryEntryMember)
                    .ToArray();

                allTargetMembers = allTargetMembers
                    .Concat(configuredCustomTargetMembers)
                    .ToArray();
            }

            return allTargetMembers;
        }

        private static IEnumerable<DictionaryTargetMember> EnumerateTargetMembers(IMemberMapperData mapperData)
        {
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;
            var sourceMembers = GlobalContext.Instance.MemberFinder.GetSourceMembers(mapperData.SourceType);

            foreach (var sourceMember in sourceMembers)
            {
                var entryTargetMember = targetDictionaryMember.Append(sourceMember.Name);

                if (!sourceMember.IsSimple)
                {
                    entryTargetMember = entryTargetMember.WithTypeOf(sourceMember);
                }

                yield return entryTargetMember;
            }
        }

        public override bool IsFor(IObjectMappingData mappingData)
        {
            if (mappingData.MapperData.TargetMember.IsDictionary)
            {
                return true;
            }

            if (mappingData.IsRoot)
            {
                return false;
            }

            var dictionaryMember = mappingData.MapperData.TargetMember as DictionaryTargetMember;

            if (dictionaryMember == null)
            {
                return false;
            }

            return dictionaryMember.HasObjectEntries || dictionaryMember.HasSimpleEntries;
        }

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
                    yield return GetDictionaryPopulation(mappingData);
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

        private static Expression GetParameterlessDictionaryAssignment(IObjectMappingData mappingData)
        {
            var newDictionary = mappingData.MapperData.TargetType.GetEmptyInstanceCreation();

            return GetDictionaryAssignment(newDictionary, mappingData);
        }

        private static Expression GetDictionaryAssignment(Expression value, IObjectMappingData mappingData)
        {
            value = mappingData.MapperData.TargetMember.IsReadOnly
                ? mappingData.MapperData.TargetObject
                : AddExistingTargetCheckIfAppropriate(value, mappingData);

            return mappingData.MapperData.InstanceVariable.AssignTo(value);
        }

        private Expression GetDictionaryPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.SourceMember.IsEnumerable)
            {
                return GetEnumerableToDictionaryMapping(mappingData);
            }

            var memberPopulations = _memberPopulationFactory
                .Create(mappingData)
                .Select(memberPopulation => memberPopulation.GetPopulation())
                .ToArray();

            if (memberPopulations.HasOne())
            {
                return memberPopulations[0];
            }

            var memberPopulationBlock = Expression.Block(memberPopulations);

            return memberPopulationBlock;
        }

        private static Expression GetEnumerableToDictionaryMapping(IObjectMappingData mappingData)
        {
            var builder = new DictionaryPopulationBuilder(mappingData.MapperData.EnumerablePopulationBuilder);

            if (builder.HasSourceEnumerable)
            {
                builder.AssignSourceVariableFromSourceObject();
            }

            builder.AddItems(mappingData);

            return builder;
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.InstanceVariable;
    }
}