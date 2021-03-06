namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries.Looping
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Enumerables.Looping;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class SourceElementsDictionaryPopulationLoopData : IPopulationLoopData
    {
        private readonly DictionaryEntryVariablePair _dictionaryVariables;
        private readonly EnumerablePopulationBuilder _builder;
        private readonly bool _performElementChecks;
        private readonly Expression _targetMemberKey;
        private readonly bool _useDirectValueAccess;
        private readonly ParameterExpression _targetElementKey;
        private readonly Expression _sourceElement;
        private ParameterExpression _elementKeyExists;

        public SourceElementsDictionaryPopulationLoopData(
            DictionaryEntryVariablePair dictionaryVariables,
            EnumerablePopulationBuilder builder)
        {
            _dictionaryVariables = dictionaryVariables;
            _builder = builder;

            var sourceDictionaryMember = dictionaryVariables.SourceMember;

            _performElementChecks =
               ElementTypesAreSimple ||
               sourceDictionaryMember.HasObjectEntries ||
              (builder.Context.ElementTypesAreAssignable && !sourceDictionaryMember.ValueType.IsSimple());

            _targetMemberKey = dictionaryVariables
                .GetTargetMemberDictionaryEnumerableElementKey(builder.Counter, MapperData);

            _useDirectValueAccess = ElementTypesAreSimple
                ? builder.Context.ElementTypesAreAssignable
                : sourceDictionaryMember.HasObjectEntries;

            var useSeparateTargetKeyVariable = !ElementTypesAreSimple && _performElementChecks;

            _targetElementKey = useSeparateTargetKeyVariable
                ? Expression.Variable(typeof(string), "targetElementKey")
                : dictionaryVariables.Key;

            _sourceElement = _useDirectValueAccess ? GetDictionaryEntryValueAccess() : dictionaryVariables.Value;
            ContinueLoopTarget = Expression.Label(typeof(void), "Continue");
            LoopExitCheck = MapperData.IsRoot ? GetRootLoopExitCheck() : GetKeyNotFoundLoopExitCheck();
        }

        #region Setup

        private Expression GetDictionaryEntryValueAccess()
            => _dictionaryVariables.GetEntryValueAccess(_targetElementKey);

        private Expression GetRootLoopExitCheck()
        {
            return ElementTypesAreSimple
                ? Expression.Not(GetContainsRootElementKeyCall())
                : GetNonSimpleElementLoopExitCheck(GetContainsRootElementKeyCall);
        }

        private Expression GetContainsRootElementKeyCall()
        {
            var containsKeyMethod = MapperData.SourceObject.Type.GetPublicInstanceMethod("ContainsKey");
            var containsKeyCall = Expression.Call(MapperData.SourceObject, containsKeyMethod, _targetElementKey);

            return containsKeyCall;
        }

        private Expression GetNonSimpleElementLoopExitCheck(Func<Expression> containsKeyElementCallFactory)
        {
            var noKeysStartWithTarget = GetNoKeysWithMatchingStartQuery();

            if (DoNotPerformElementChecks)
            {
                return noKeysStartWithTarget;
            }

            var containsElementKeyCall = containsKeyElementCallFactory.Invoke();
            _elementKeyExists = Expression.Variable(typeof(bool), "elementKeyExists");
            var assignElementKeyExists = _elementKeyExists.AssignTo(containsElementKeyCall);
            var elementKeyDoesNotExist = Expression.Not(assignElementKeyExists);

            return Expression.AndAlso(elementKeyDoesNotExist, noKeysStartWithTarget);
        }

        private Expression GetKeyNotFoundLoopExitCheck()
        {
            if (ElementTypesAreSimple)
            {
                return GetContainsElementKeyCheck().GetIsDefaultComparison();
            }

            return GetNonSimpleElementLoopExitCheck(
                () => GetContainsElementKeyCheck().GetIsNotDefaultComparison());
        }

        private Expression GetContainsElementKeyCheck()
            => _dictionaryVariables.GetMatchingKeyAssignment(_targetElementKey);

        private Expression GetNoKeysWithMatchingStartQuery()
            => _dictionaryVariables.GetNoKeysWithMatchingStartQuery(_targetElementKey);

        #endregion

        private ObjectMapperData MapperData => _builder.MapperData;

        public bool NeedsContinueTarget { get; set; }

        public LabelTarget ContinueLoopTarget { get; }

        public Expression LoopExitCheck { get; }

        public Expression GetSourceElementValue() => _sourceElement;

        public Expression GetElementMapping(IObjectMappingData enumerableMappingData)
        {
            if (DoNotPerformElementChecks)
            {
                return GetElementMappingBlock(GetDictionaryToElementMapping(enumerableMappingData));
            }

            var entryToElementMapping = _builder.GetElementConversion(_sourceElement, enumerableMappingData);

            if (ElementTypesAreSimple)
            {
                return _useDirectValueAccess
                    ? entryToElementMapping
                    : GetElementMappingBlock(entryToElementMapping);
            }

            var dictionaryToElementMapping = GetDictionaryToElementMapping(enumerableMappingData);

            var mapElementOrElementMembers = Expression.Condition(
                _elementKeyExists,
                entryToElementMapping,
                dictionaryToElementMapping);

            return GetElementMappingBlock(mapElementOrElementMembers);
        }

        private Expression GetDictionaryToElementMapping(IObjectMappingData enumerableMappingData)
        {
            var elementMappingData = ObjectMappingDataFactory.ForElement(
                MapperData.SourceType,
                _builder.Context.TargetElementType,
                enumerableMappingData);

            var dictionaryToElementMapping = MappingFactory.GetElementMapping(
                MapperData.SourceObject,
                _builder.Context.TargetElementType.ToDefaultExpression(),
                elementMappingData);

            return dictionaryToElementMapping;
        }

        private bool ElementTypesAreSimple => _builder.TargetElementsAreSimple;

        private bool DoNotPerformElementChecks => !_performElementChecks;

        private Expression GetElementMappingBlock(Expression elementMapping)
        {
            if (_useDirectValueAccess || DoNotPerformElementChecks)
            {
                return elementMapping;
            }

            var dictionaryValueAccess = GetDictionaryEntryValueAccess();
            var dictionaryEntryAssignment = _sourceElement.AssignTo(dictionaryValueAccess);

            return Expression.Block(
                new[] { (ParameterExpression)_sourceElement },
                dictionaryEntryAssignment,
                elementMapping);
        }

        public Expression Adapt(LoopExpression loop)
        {
            loop = loop
                .InsertAssignment(Constants.BeforeLoopExitCheck, _dictionaryVariables.Key, _targetElementKey)
                .InsertAssignment(Constants.BeforeLoopExitCheck, _targetElementKey, _targetMemberKey);

            var loopBody = (BlockExpression)loop.Body;

            IList<ParameterExpression> loopVariables = loopBody.Variables;

            if (_elementKeyExists != null)
            {
                loopVariables = loopVariables.Append(_elementKeyExists);
            }

            if (ReferenceEquals(loopVariables, loopBody.Variables))
            {
                return loop;
            }

            loopBody = loopBody.Update(loopVariables, loopBody.Expressions);

            loop = loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);

            return loop;
        }
    }
}