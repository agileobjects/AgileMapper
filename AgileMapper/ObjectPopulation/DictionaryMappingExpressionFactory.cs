namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions;

    internal class DictionaryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public override bool IsFor(IObjectMappingData mappingData)
        {
            while (mappingData != null)
            {
                if (mappingData.MapperKey.MappingTypes.TargetType.IsDictionary())
                {
                    return true;
                }

                mappingData = mappingData.Parent;
            }

            return false;
        }

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
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
                Expression cloneDictionary;

                if (SourceMemberIsSameTypeDictionary(mapperData, out cloneDictionary))
                {
                    yield return mapperData.InstanceVariable.AssignTo(cloneDictionary);
                    yield break;
                }

                yield return mapperData.InstanceVariable.AssignTo(Expression.New(mapperData.TargetType));
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var allTargetMembers = EnumerateTargetMembers(mapperData.SourceType, targetDictionaryMember);
            var memberPopulations = MemberPopulationFactory.Create(allTargetMembers, mappingData);

            foreach (var memberPopulation in memberPopulations)
            {
                yield return memberPopulation.GetPopulation();
            }
        }

        private bool SourceMemberIsSameTypeDictionary(IMemberMapperData mapperData, out Expression cloneDictionary)
        {
            var sourceDictionaryMember = mapperData.GetDictionarySourceMemberOrNull();

            if ((sourceDictionaryMember == null) ||
                (mapperData.TargetMember.Type != sourceDictionaryMember.Type))
            {
                cloneDictionary = null;
                return false;
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;
            var copyConstructor = GetDictionaryCopyConstructor(targetDictionaryMember.Type);
            var comparer = Expression.Property(mapperData.SourceObject, "Comparer");
            cloneDictionary = Expression.New(copyConstructor, mapperData.SourceObject, comparer);
            return true;
        }

        private static ConstructorInfo GetDictionaryCopyConstructor(Type dictionaryType)
        {
            var dictionaryTypes = dictionaryType.GetGenericArguments();
            var dictionaryInterfaceType = typeof(IDictionary<,>).MakeGenericType(dictionaryTypes);

            var copyConstructor = dictionaryType
              .GetPublicInstanceConstructors()
              .Select(ctor => new { Ctor = ctor, Parameters = ctor.GetParameters() })
              .First(ctor =>
                  (ctor.Parameters.Length == 2) &&
                  (ctor.Parameters[0].ParameterType == dictionaryInterfaceType))
              .Ctor;

            return copyConstructor;
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