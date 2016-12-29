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
        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperKey.MappingTypes.TargetType.IsDictionary();

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

            if (mapperData.TargetType.IsDictionary())
            {
                DictionarySourceMember sourceDictionaryMember;

                if (SourceMemberIsDictionary(mapperData, out sourceDictionaryMember))
                {
                    if (UseDictionaryCloneConstructor(sourceDictionaryMember, mapperData))
                    {
                        yield return GetClonedDictionaryAssignment(mapperData);
                        yield break;
                    }

                    yield return GetProjectedDictionaryAssignment(sourceDictionaryMember, mappingData);
                    yield return GetDictionaryToDictionaryProjection(sourceDictionaryMember, mappingData);
                    yield break;
                }

                yield return GetParameterlessDictionaryAssignment(mappingData);
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var allTargetMembers = EnumerateTargetMembers(mapperData.SourceType, targetDictionaryMember);
            var memberPopulations = MemberPopulationFactory.Create(allTargetMembers, mappingData);

            foreach (var memberPopulation in memberPopulations)
            {
                yield return memberPopulation.GetPopulation();
            }
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
            if (sourceDictionaryMember.Type != mapperData.TargetType)
            {
                return false;
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            return targetDictionaryMember.ValueType.IsSimple();
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

        private static Expression GetProjectedDictionaryAssignment(
            DictionarySourceMember sourceDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            if (sourceDictionaryMember.ValueType != targetDictionaryMember.ValueType)
            {
                return GetParameterlessDictionaryAssignment(mappingData);
            }

            var comparer = Expression.Property(mapperData.SourceObject, "Comparer");

            var constructor = FindDictionaryConstructor(
                targetDictionaryMember.Type,
                comparer.Type,
                numberOfParameters: 1);

            return GetDictionaryAssignment(Expression.New(constructor, comparer), mappingData);
        }

        private static Expression GetDictionaryToDictionaryProjection(
            DictionarySourceMember sourceDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var keyValuePairType = typeof(KeyValuePair<,>)
                .MakeGenericType(sourceDictionaryMember.KeyType, sourceDictionaryMember.ValueType);

            var populationLoopData = new EnumerableSourcePopulationLoopData(
                mapperData.EnumerablePopulationBuilder,
                keyValuePairType,
                mapperData.SourceObject);

            var populationLoop = populationLoopData.BuildPopulationLoop(
                mapperData.EnumerablePopulationBuilder,
                mappingData,
                GetTargetEntryAssignment);

            return populationLoop;
        }

        private static Expression GetTargetEntryAssignment(IPopulationLoopData loopData, IObjectMappingData mappingData)
        {
            var populationLoopData = (EnumerableSourcePopulationLoopData)loopData;
            var mapperData = populationLoopData.Builder.MapperData;
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var keyVariable = Expression.Variable(targetDictionaryMember.KeyType, "targetKey");
            var keyAccess = Expression.Property(populationLoopData.SourceElement, "Key");
            var keyConversion = mapperData.GetValueConversion(keyAccess, keyVariable.Type);
            var keyAssignment = keyVariable.AssignTo(keyConversion);

            var valueAccess = Expression.Property(populationLoopData.SourceElement, "Value");
            var valueConversion = populationLoopData.Builder.GetElementConversion(valueAccess, mappingData);

            var targetEntryIndex = mapperData.InstanceVariable.GetIndexAccess(keyVariable);
            var targetEntryAssignment = targetEntryIndex.AssignTo(valueConversion);

            return Expression.Block(new[] { keyVariable }, keyAssignment, targetEntryAssignment);
        }

        private static Expression GetParameterlessDictionaryAssignment(IObjectMappingData mappingData)
            => GetDictionaryAssignment(Expression.New(mappingData.MapperData.TargetType), mappingData);

        private static Expression GetDictionaryAssignment(Expression value, IObjectMappingData mappingData)
        {
            value = AddExistingTargetCheckIfAppropriate(value, mappingData);

            return mappingData.MapperData.InstanceVariable.AssignTo(value);
        }

        private static IEnumerable<QualifiedMember> EnumerateTargetMembers(
            Type parentSourceType,
            DictionaryTargetMember targetDictionaryMember)
        {
            var sourceMembers = GlobalContext.Instance.MemberFinder.GetSourceMembers(parentSourceType);

            foreach (var sourceMember in sourceMembers)
            {
                var entryTargetMember = targetDictionaryMember.Append(sourceMember.Name);

                if (sourceMember.IsSimple)
                {
                    yield return entryTargetMember;
                    continue;
                }

                foreach (var childTargetMember in EnumerateTargetMembers(sourceMember.Type, entryTargetMember))
                {
                    yield return childTargetMember;
                }
            }
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;
    }
}