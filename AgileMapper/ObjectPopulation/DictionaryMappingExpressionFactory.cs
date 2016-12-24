namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
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

            if (mapperData.IsRoot)
            {
                var objectValue = Expression.New(mapperData.TargetType);

                var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, objectValue);
                yield return instanceVariableAssignment;
            }

            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            foreach (var population in GetMemberPopulations(mapperData.SourceType, targetDictionaryMember, mappingData))
            {
                yield return population;
            }
        }

        private static IEnumerable<Expression> GetMemberPopulations(
            Type parentType,
            DictionaryTargetMember targetDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceMembers = GlobalContext.Instance.MemberFinder.GetSourceMembers(parentType);

            foreach (var sourceMember in sourceMembers)
            {
                var qualifiedSourceMember = mapperData.SourceMember.Append(sourceMember);
                var entryTargetMember = targetDictionaryMember.Append(sourceMember.Name);

                if (sourceMember.IsSimple)
                {
                    yield return GetSimpleMemberPopulation(
                        qualifiedSourceMember,
                        entryTargetMember,
                        targetDictionaryMember,
                        mapperData);

                    continue;
                }

                if (sourceMember.IsComplex)
                {
                    yield return GetNestedMemberPopulations(
                        qualifiedSourceMember,
                        entryTargetMember,
                        mappingData);
                }
            }
        }

        private static Expression GetSimpleMemberPopulation(
            IQualifiedMember sourceMember,
            DictionaryTargetMember entryTargetMember,
            DictionaryTargetMember targetDictionaryMember,
            ObjectMapperData mapperData)
        {
            var entryMapperData = new ChildMemberMapperData(entryTargetMember, mapperData);
            var sourceMemberDataSource = SourceMemberDataSource.For(sourceMember, entryMapperData);
            var population = targetDictionaryMember.GetPopulation(sourceMemberDataSource.Value, entryMapperData);

            return population;
        }

        private static Expression GetNestedMemberPopulations(
            IQualifiedMember sourceMember,
            QualifiedMember entryTargetMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var childMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                entryTargetMember,
                0,
                mappingData);

            var mappingValues = new MappingValues(
                sourceMember.GetQualifiedAccess(mapperData.SourceObject),
                entryTargetMember.Type.ToDefaultExpression(),
                mapperData.EnumerableIndex);

            var childMappingBlock = (TryExpression)MappingFactory.GetChildMapping(
                childMappingData,
                mappingValues,
                0,
                mapperData);

            var mappingBlock = (BlockExpression)childMappingBlock.Body;

            mappingBlock = Expression.Block(
                mappingBlock.Expressions.Take(mappingBlock.Expressions.Count - 1));

            return mappingBlock;
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;
    }
}