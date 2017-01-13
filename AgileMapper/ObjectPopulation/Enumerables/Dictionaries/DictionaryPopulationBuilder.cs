namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using Members.Population;

    internal class DictionaryPopulationBuilder
    {
        private readonly EnumerablePopulationBuilder _wrappedBuilder;
        private readonly DictionarySourceMember _sourceDictionaryMember;
        private readonly DictionaryTargetMember _targetDictionaryMember;
        private readonly List<Expression> _mappingExpressions;

        public DictionaryPopulationBuilder(EnumerablePopulationBuilder wrappedBuilder)
        {
            _wrappedBuilder = wrappedBuilder;
            _sourceDictionaryMember = wrappedBuilder.MapperData.GetDictionarySourceMemberOrNull();
            _targetDictionaryMember = (DictionaryTargetMember)MapperData.TargetMember;
            _mappingExpressions = new List<Expression>();
        }

        #region Operator

        public static implicit operator BlockExpression(DictionaryPopulationBuilder builder)
        {
            var mappingBlock = (BlockExpression)builder._wrappedBuilder;

            if (builder._mappingExpressions.None())
            {
                return mappingBlock;
            }

            if (mappingBlock != null)
            {
                builder._mappingExpressions.InsertRange(0, mappingBlock.Expressions);
            }

            return Expression.Block(builder._mappingExpressions);
        }

        #endregion

        public bool HasSourceEnumerable => !HasSourceDictionary;

        public bool HasSourceDictionary => _sourceDictionaryMember != null;

        private ObjectMapperData MapperData => _wrappedBuilder.MapperData;

        public void AssignSourceVariableFromSourceObject()
        {
            _wrappedBuilder.AssignSourceVariableFromSourceObject();
        }

        public void AddItems(IObjectMappingData mappingData)
        {
            if (HasSourceDictionary)
            {
                BuildDictionaryToDictionaryPopulationLoop(mappingData);
                return;
            }

            _wrappedBuilder.BuildPopulationLoop(AssignDictionaryEntry, mappingData);
        }

        private void BuildDictionaryToDictionaryPopulationLoop(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var loopData = new DictionaryToDictionaryPopulationLoopData(_sourceDictionaryMember, mapperData);

            var populationLoop = loopData.BuildPopulationLoop(
                mapperData.EnumerablePopulationBuilder,
                mappingData,
                AssignDictionaryEntryFromKeyValuePair);

            _mappingExpressions.Add(populationLoop);
        }

        private Expression AssignDictionaryEntryFromKeyValuePair(
            DictionaryToDictionaryPopulationLoopData loopData,
            IObjectMappingData mappingData)
        {
            var keyVariable = Expression.Variable(_targetDictionaryMember.KeyType, "targetKey");
            var keyAccess = Expression.Property(loopData.SourceElement, "Key");
            var keyConversion = MapperData.GetValueConversion(keyAccess, keyVariable.Type);
            var keyAssignment = keyVariable.AssignTo(keyConversion);

            var dictionaryEntryMember = _targetDictionaryMember.Append(keyVariable);
            var targetEntryAssignment = AssignDictionaryEntry(loopData, dictionaryEntryMember, mappingData);

            return Expression.Block(new[] { keyVariable }, keyAssignment, targetEntryAssignment);
        }

        private Expression AssignDictionaryEntry(IPopulationLoopData loopData, IObjectMappingData mappingData)
        {
            var dictionaryEntryMember = _targetDictionaryMember.GetElementMember();

            return AssignDictionaryEntry(loopData, dictionaryEntryMember, mappingData);
        }

        private Expression AssignDictionaryEntry(
            IPopulationLoopData loopData,
            QualifiedMember dictionaryEntryMember,
            IObjectMappingData mappingData)
        {
            mappingData = GetMappingData(mappingData);

            var elementMapperData = new ChildMemberMapperData(dictionaryEntryMember, MapperData);
            var elementMapping = loopData.GetElementMapping(mappingData);
            var elementMappingData = mappingData.GetChildMappingData(elementMapperData);

            var mapperData = mappingData.IsRoot ? mappingData.MapperData : mappingData.MapperData.Parent;
            var sourceMemberDataSource = SourceMemberDataSource.For(mapperData.SourceMember, mapperData);
            var mappingDataSource = new AdHocDataSource(sourceMemberDataSource, elementMapping);
            var mappingDataSources = new DataSourceSet(mappingDataSource);

            var memberPopulation = new MemberPopulation(elementMappingData, mappingDataSources, null);
            var populationExpression = memberPopulation.GetPopulation();

            return populationExpression;
        }

        private IObjectMappingData GetMappingData(IObjectMappingData mappingData)
        {
            if (_wrappedBuilder.ElementTypesAreSimple)
            {
                return mappingData;
            }

            var sourceElementType = _wrappedBuilder.Context.SourceElementType;
            var targetElementType = _targetDictionaryMember.GetElementType(sourceElementType);

            mappingData = ObjectMappingDataFactory.ForElement(sourceElementType, targetElementType, mappingData);

            return mappingData;
        }
    }
}