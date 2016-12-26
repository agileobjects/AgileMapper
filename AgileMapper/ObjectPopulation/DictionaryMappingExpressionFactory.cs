namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
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
            var targetMember = mappingData.MapperData.TargetMember as DictionaryTargetMember;

            if (targetMember == null)
            {
                nullMappingBlock = null;
                return false;
            }

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

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
            => Enumerable<Expression>.Empty;

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetType.IsDictionary())
            {
                var objectValue = Expression.New(mapperData.TargetType);

                var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, objectValue);
                yield return instanceVariableAssignment;
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var allTargetMembers = EnumerateTargetMembers(mapperData.SourceType, targetDictionaryMember);
            var memberPopulations = MemberPopulationFactory.Create(allTargetMembers, mappingData);

            foreach (var memberPopulation in memberPopulations)
            {
                yield return memberPopulation.GetPopulation();
            }
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