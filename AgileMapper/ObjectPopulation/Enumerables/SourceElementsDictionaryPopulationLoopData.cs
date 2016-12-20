namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using DataSources;
    using Extensions;
    using Members;

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
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
            : this(new DictionaryEntryVariablePair(sourceMember, builder.MapperData), builder)
        {
        }

        public SourceElementsDictionaryPopulationLoopData(
            DictionaryEntryVariablePair dictionaryVariables,
            EnumerablePopulationBuilder builder)
        {
            _dictionaryVariables = dictionaryVariables;
            _builder = builder;

            var sourceMember = dictionaryVariables.SourceMember;

            DoNotPerformElementChecks =
                !ElementTypesAreSimple &&
                !sourceMember.HasObjectEntries &&
                (sourceMember.EntryType.IsSimple() || !builder.Context.ElementTypesAreAssignable);

            _targetMemberKey = dictionaryVariables
                .GetTargetMemberDictionaryEnumerableElementKey(builder);

            _useDirectValueAccess = ElementTypesAreSimple
                ? builder.Context.ElementTypesAreAssignable
                : sourceMember.HasObjectEntries;

            var useSeparateTargetKeyVariable = !ElementTypesAreSimple && PerformElementChecks;

            _targetElementKey = useSeparateTargetKeyVariable
                ? Expression.Variable(typeof(string), "targetKey")
                : dictionaryVariables.Key;

            _sourceElement = _useDirectValueAccess ? GetDictionaryEntryValueAccess() : dictionaryVariables.Value;
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
            var containsKeyMethod = MapperData.SourceObject.Type.GetMethod("ContainsKey");
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
            var assignElementKeyExists = Expression.Assign(_elementKeyExists, containsElementKeyCall);
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

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
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
                Expression.Default(_builder.Context.TargetElementType),
                elementMappingData);

            return dictionaryToElementMapping;
        }

        private bool ElementTypesAreSimple => _builder.ElementTypesAreSimple;

        private bool PerformElementChecks => !DoNotPerformElementChecks;

        private bool DoNotPerformElementChecks { get; }

        private Expression GetElementMappingBlock(Expression elementMapping)
        {
            if (_useDirectValueAccess || DoNotPerformElementChecks)
            {
                return elementMapping;
            }

            var dictionaryValueAccess = GetDictionaryEntryValueAccess();
            var dictionaryEntryAssignment = Expression.Assign(_sourceElement, dictionaryValueAccess);

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