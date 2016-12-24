namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using ReadableExpressions;

    internal class DictionaryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperKey.MappingTypes.TargetType.IsDictionary();

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

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
            => Enumerable<Expression>.Empty;

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var objectValue = Expression.New(mapperData.TargetType);

            var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, objectValue);
            yield return instanceVariableAssignment;

            var sourceMembers = GlobalContext.Instance.MemberFinder.GetSourceMembers(mapperData.SourceType);

            foreach (var sourceMember in sourceMembers)
            {
                var qualifiedSourceMember = mapperData.SourceMember.Append(sourceMember);

                var targetEntryMember = Member.DictionaryEntry(sourceMember, targetDictionaryMember);
                var qualifiedTargetMember = targetDictionaryMember.Append(targetEntryMember);
                var entryMapperData = new ChildMemberMapperData(qualifiedTargetMember, mapperData);

                var sourceMemberDataSource = new SourceMemberDataSource(qualifiedSourceMember, entryMapperData);

                var population = targetEntryMember.GetPopulation(mapperData.InstanceVariable, sourceMemberDataSource.Value);

                yield return population;
            }
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;
    }
}