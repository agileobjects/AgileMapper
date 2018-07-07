namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using Members.Population;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            if (builder._mappingExpressions.None())
            {
                return builder._wrappedBuilder;
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
            loopData.NeedsContinueTarget = true;

            var dictionaryEntryMember = _targetDictionaryMember.GetElementMember();

            return AssignDictionaryEntry(loopData, dictionaryEntryMember, mappingData);
        }

        private Expression AssignDictionaryEntry(
            IPopulationLoopData loopData,
            DictionaryTargetMember dictionaryEntryMember,
            IObjectMappingData mappingData)
        {
            if (_wrappedBuilder.ElementTypesAreSimple)
            {
                return GetPopulation(loopData, dictionaryEntryMember, mappingData);
            }

            mappingData = GetMappingData(mappingData);

            if (dictionaryEntryMember.HasComplexEntries)
            {
                return GetPopulation(loopData, dictionaryEntryMember, mappingData);
            }

            var derivedSourceTypes = mappingData.MapperData.GetDerivedSourceTypes();
            var hasDerivedSourceTypes = derivedSourceTypes.Any();

            List<ParameterExpression> typedVariables;
            List<Expression> mappingExpressions;

            if (hasDerivedSourceTypes)
            {
                typedVariables = new List<ParameterExpression>(derivedSourceTypes.Count);
                mappingExpressions = new List<Expression>(typedVariables.Count * 2 + 2);

                AddDerivedSourceTypePopulations(
                    loopData,
                    dictionaryEntryMember,
                    mappingData,
                    derivedSourceTypes,
                    typedVariables,
                    mappingExpressions);
            }
            else
            {
                typedVariables = null;
                mappingExpressions = new List<Expression>(2);
            }

            InsertSourceElementNullCheck(loopData, mappingExpressions);

            mappingExpressions.Add(GetPopulation(loopData, dictionaryEntryMember, mappingData));

            var mappingBlock = hasDerivedSourceTypes
                ? Expression.Block(typedVariables, mappingExpressions)
                : Expression.Block(mappingExpressions);

            return mappingBlock;
        }

        private void AddDerivedSourceTypePopulations(
            IPopulationLoopData loopData,
            QualifiedMember dictionaryEntryMember,
            IObjectMappingData mappingData,
            IEnumerable<Type> derivedSourceTypes,
            ICollection<ParameterExpression> typedVariables,
            ICollection<Expression> mappingExpressions)
        {
            var sourceElement = loopData.GetSourceElementValue();
            var mapNextElement = Expression.Continue(loopData.ContinueLoopTarget);

            var orderedDerivedSourceTypes = derivedSourceTypes
                .OrderBy(t => t, TypeComparer.MostToLeastDerived);

            foreach (var derivedSourceType in orderedDerivedSourceTypes)
            {
                var derivedSourceCheck = new DerivedSourceTypeCheck(derivedSourceType);
                var typedVariableAssignment = derivedSourceCheck.GetTypedVariableAssignment(sourceElement);

                typedVariables.Add(derivedSourceCheck.TypedVariable);
                mappingExpressions.Add(typedVariableAssignment);

                var derivedTypeMapping = GetDerivedTypeMapping(derivedSourceCheck, mappingData);
                var derivedTypePopulation = GetPopulation(derivedTypeMapping, dictionaryEntryMember, mappingData);
                var incrementCounter = _wrappedBuilder.GetCounterIncrement();
                var derivedMappingBlock = Expression.Block(derivedTypePopulation, incrementCounter, mapNextElement);
                var ifDerivedTypeReturn = Expression.IfThen(derivedSourceCheck.TypeCheck, derivedMappingBlock);

                mappingExpressions.Add(ifDerivedTypeReturn);
            }
        }

        private static void InsertSourceElementNullCheck(IPopulationLoopData loopData, IList<Expression> mappingExpressions)
        {
            var sourceElement = loopData.GetSourceElementValue();

            if (sourceElement.Type.CannotBeNull())
            {
                return;
            }

            loopData.NeedsContinueTarget = true;

            var sourceElementIsNull = sourceElement.GetIsDefaultComparison();
            var continueLoop = Expression.Continue(loopData.ContinueLoopTarget);
            var ifNullContinue = Expression.IfThen(sourceElementIsNull, continueLoop);

            mappingExpressions.Insert(0, ifNullContinue);
        }

        private Expression GetPopulation(
            IPopulationLoopData loopData,
            QualifiedMember dictionaryEntryMember,
            IObjectMappingData dictionaryMappingData)
        {
            var elementMapping = loopData.GetElementMapping(dictionaryMappingData);

            return GetPopulation(elementMapping, dictionaryEntryMember, dictionaryMappingData);
        }

        private Expression GetPopulation(
            Expression elementMapping,
            QualifiedMember dictionaryEntryMember,
            IObjectMappingData mappingData)
        {
            var elementMapperData = new ChildMemberMapperData(dictionaryEntryMember, MapperData);
            var elementMappingData = mappingData.GetChildMappingData(elementMapperData);

            var sourceMember = mappingData.MapperData.SourceMember;
            var mappingDataSource = new AdHocDataSource(sourceMember, elementMapping);
            var mappingDataSources = new DataSourceSet(elementMapperData, mappingDataSource);

            var memberPopulation = MemberPopulator.WithoutRegistration(elementMappingData, mappingDataSources);
            var populationExpression = memberPopulation.GetPopulation();

            return populationExpression;
        }

        private IObjectMappingData GetMappingData(IObjectMappingData mappingData)
        {
            var sourceElementType = _wrappedBuilder.Context.SourceElementType;
            var targetElementType = _targetDictionaryMember.GetElementType(sourceElementType);

            mappingData = ObjectMappingDataFactory.ForElement(sourceElementType, targetElementType, mappingData);

            return mappingData;
        }

        private static Expression GetDerivedTypeMapping(
            DerivedSourceTypeCheck derivedSourceCheck,
            IObjectMappingData mappingData)
        {
            var mappingTryCatch = DerivedMappingFactory.GetDerivedTypeMapping(
                mappingData,
                derivedSourceCheck.TypedVariable,
                mappingData.MapperData.TargetType);

            return mappingTryCatch;
        }
    }
}