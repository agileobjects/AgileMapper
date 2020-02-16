﻿namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using Members.Population;
    using TypeConversion;

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
            var keyVariable = (ParameterExpression)_wrappedBuilder.GetElementKey();
            var keyAccess = Expression.Property(loopData.SourceElement, "Key");
            var keyConversion = MapperData.GetValueConversion(keyAccess, keyVariable.Type);
            var keyAssignment = keyVariable.AssignTo(keyConversion);

            var dictionaryEntryMember = _targetDictionaryMember.Append(keyVariable);
            var targetEntryAssignment = AssignDictionaryEntry(loopData, dictionaryEntryMember, mappingData);

            if (targetEntryAssignment.NodeType != ExpressionType.Block)
            {
                return Expression.Block(new[] { keyVariable }, keyAssignment, targetEntryAssignment);
            }

            var targetEntryAssignmentBlock = (BlockExpression)targetEntryAssignment;

            return Expression.Block(
                targetEntryAssignmentBlock.Variables.Append(keyVariable),
                targetEntryAssignmentBlock.Expressions.Prepend(keyAssignment));
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
            if (_wrappedBuilder.TargetElementsAreSimple)
            {
                return GetPopulation(loopData, dictionaryEntryMember, mappingData);
            }

            mappingData = GetMappingData(mappingData);

            if (dictionaryEntryMember.HasComplexEntries)
            {
                return GetPopulation(loopData, dictionaryEntryMember, mappingData);
            }

            List<ParameterExpression> typedVariables;
            List<Expression> mappingExpressions;

            var derivedSourceTypes = mappingData.MapperData.GetDerivedSourceTypes();
            var hasDerivedSourceTypes = derivedSourceTypes.Any();

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

            mappingExpressions.Add(GetPopulation(loopData, dictionaryEntryMember, mappingData));

            InsertSourceElementNullCheck(
                loopData,
                dictionaryEntryMember,
                mappingData.MapperData,
                mappingExpressions);

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

        private void InsertSourceElementNullCheck(
            IPopulationLoopData loopData,
            DictionaryTargetMember dictionaryEntryMember,
            IMemberMapperData mapperData,
            IList<Expression> mappingExpressions)
        {
            var sourceElement = loopData.GetSourceElementValue();

            if (sourceElement.Type.CannotBeNull())
            {
                return;
            }

            loopData.NeedsContinueTarget = true;

            var sourceElementIsNull = sourceElement.GetIsDefaultComparison();

            var nullTargetValue = dictionaryEntryMember.ValueType.ToDefaultExpression();
            var addNullEntry = dictionaryEntryMember.GetPopulation(nullTargetValue, mapperData);

            var incrementCounter = _wrappedBuilder.GetCounterIncrement();
            var continueLoop = Expression.Continue(loopData.ContinueLoopTarget);

            var nullEntryActions = Expression.Block(addNullEntry, incrementCounter, continueLoop);

            var ifNullContinue = Expression.IfThen(sourceElementIsNull, nullEntryActions);

            mappingExpressions.Insert(0, ifNullContinue);
        }

        private Expression GetPopulation(
            IPopulationLoopData loopData,
            DictionaryTargetMember dictionaryEntryMember,
            IObjectMappingData dictionaryMappingData)
        {
            var elementMapping = loopData.GetElementMapping(dictionaryMappingData);

            if (elementMapping == Constants.EmptyExpression)
            {
                return elementMapping;
            }

            if (dictionaryEntryMember.HasKey &&
                dictionaryEntryMember.CheckExistingElementValue &&
                dictionaryMappingData.MapperData.TargetCouldBePopulated())
            {
                elementMapping = elementMapping.Replace(
                    dictionaryMappingData.MapperData.GetTargetMemberDictionaryKey(),
                    dictionaryEntryMember.Key,
                    ExpressionEvaluation.Equivalator);
            }

            return GetPopulation(elementMapping, dictionaryEntryMember, dictionaryMappingData);
        }

        private Expression GetPopulation(
            Expression elementMapping,
            QualifiedMember dictionaryEntryMember,
            IObjectMappingData mappingData)
        {
            var elementMapperData = new ChildMemberMapperData(dictionaryEntryMember, MapperData);

            var sourceMember = mappingData.MapperData.SourceMember;
            var mappingDataSource = new AdHocDataSource(sourceMember, elementMapping);
            var mappingDataSources = DataSourceSet.For(mappingDataSource, elementMapperData);
            var populator = new MemberPopulator(mappingDataSources, elementMapperData);
            var populationExpression = populator.GetPopulation();

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