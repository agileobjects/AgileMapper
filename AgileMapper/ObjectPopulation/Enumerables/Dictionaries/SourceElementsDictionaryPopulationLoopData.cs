namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using DataSources;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SourceElementsDictionaryPopulationLoopData : IPopulationLoopData
    {
        private readonly DictionaryEntryVariablePair _dictionaryVariables;
        private readonly EnumerablePopulationBuilder _builder;
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

            var sourceMember = dictionaryVariables.SourceMember;

            PerformElementChecks =
               ElementTypesAreSimple ||
               sourceMember.HasObjectEntries ||
              (builder.Context.ElementTypesAreAssignable && !sourceMember.ValueType.IsSimple());

            _targetMemberKey = dictionaryVariables
                .GetTargetMemberDictionaryEnumerableElementKey(builder.Counter, MapperData);

            _useDirectValueAccess = ElementTypesAreSimple
                ? builder.Context.ElementTypesAreAssignable
                : sourceMember.HasObjectEntries;

            var useSeparateTargetKeyVariable = !ElementTypesAreSimple && PerformElementChecks;

            _targetElementKey = useSeparateTargetKeyVariable
                ? Expression.Variable(typeof(string), "targetKey")
                : dictionaryVariables.Key;

            _sourceElement = _useDirectValueAccess ? GetDictionaryEntryValueAccess() : dictionaryVariables.Value;
            ContinueLoopTarget = Expression.Label(typeof(void), "Continue");
            LoopExitCheck = MapperData.IsRoot ? GetRootLoopExitCheck() : GetKeyNotFoundLoopExitCheck();
        }

        private ObjectMapperData MapperData => _builder.MapperData;

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

        private bool ElementTypesAreSimple => _builder.ElementTypesAreSimple;

        private bool PerformElementChecks { get; }

        private bool DoNotPerformElementChecks => !PerformElementChecks;

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
            loop = loop.InsertAssignment(Constants.BeforeLoopExitCheck, _targetElementKey, _targetMemberKey);

            var loopBody = (BlockExpression)loop.Body;

            var loopVariables = new List<ParameterExpression>(loopBody.Variables);

            if (_elementKeyExists != null)
            {
                loopVariables.Add(_elementKeyExists);

                if (PerformElementChecks && !MapperData.IsRoot)
                {
                    loopVariables.Add(_dictionaryVariables.Key);
                }
            }

            if (loopVariables.Count == loopBody.Variables.Count)
            {
                return loop;
            }

            loopBody = loopBody.Update(loopVariables, loopBody.Expressions);

            loop = loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);

            return loop;
        }
    }
}