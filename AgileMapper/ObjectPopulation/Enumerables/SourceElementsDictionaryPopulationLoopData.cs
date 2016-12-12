namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class SourceElementsDictionaryPopulationLoopData : IPopulationLoopData
    {
        private readonly DictionaryEntryVariablePair _dictionaryVariables;
        private readonly EnumerablePopulationBuilder _builder;
        private readonly Expression _targetMemberKey;
        private readonly bool _useDirectValueAccess;
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
                .GetTargetMemberDictionaryEnumerableElementKey(MapperData, builder.Counter);

            _useDirectValueAccess = ElementTypesAreSimple
                ? builder.Context.ElementTypesAreAssignable
                : sourceMember.HasObjectEntries;

            TargetElementKey = Expression.Variable(typeof(string), "targetKey");
            SourceElement = _useDirectValueAccess ? GetDictionaryEntryValueAccess() : dictionaryVariables.Value;
            LoopExitCheck = MapperData.IsRoot ? GetRootLoopExitCheck() : GetKeyNotFoundLoopExitCheck();
        }

        private ObjectMapperData MapperData => _builder.MapperData;

        private Expression GetDictionaryEntryValueAccess()
        {
            return MapperData.IsRoot
                ? _dictionaryVariables.GetEntryValueAccess(MapperData, TargetElementKey)
                : _dictionaryVariables.GetEntryValueAccess(MapperData);
        }

        private Expression GetRootLoopExitCheck()
        {
            if (_builder.Context.ElementTypesAreSimple)
            {
                return Expression.Not(GetContainsRootElementKeyCall());
            }

            var noKeysStartWithTarget = GetNoKeysWithMatchingStartQuery();

            if (DoNotPerformElementChecks)
            {
                return noKeysStartWithTarget;
            }

            var containsElementKeyCall = GetContainsRootElementKeyCall();
            _elementKeyExists = Expression.Variable(typeof(bool), "elementKeyExists");
            var assignElementKeyExists = Expression.Assign(_elementKeyExists, containsElementKeyCall);
            var elementKeyDoesNotExist = Expression.Not(assignElementKeyExists);

            return Expression.AndAlso(elementKeyDoesNotExist, noKeysStartWithTarget);
        }

        private Expression GetContainsRootElementKeyCall()
        {
            var containsKeyMethod = MapperData.SourceObject.Type.GetMethod("ContainsKey");
            var containsKeyCall = Expression.Call(MapperData.SourceObject, containsKeyMethod, TargetElementKey);

            return containsKeyCall;
        }

        private Expression GetKeyNotFoundLoopExitCheck()
        {
            if (_builder.Context.ElementTypesAreSimple)
            {
                return GetContainsElementKeyCheck().GetIsDefaultComparison();
            }

            var noKeysStartWithTarget = GetNoKeysWithMatchingStartQuery();

            if (DoNotPerformElementChecks)
            {
                return noKeysStartWithTarget;
            }

            var containsElementKeyCheck = GetContainsElementKeyCheck().GetIsNotDefaultComparison();
            _elementKeyExists = Expression.Variable(typeof(bool), "elementKeyExists");
            var assignElementKeyExists = Expression.Assign(_elementKeyExists, containsElementKeyCheck);
            var elementKeyDoesNotExist = Expression.Not(assignElementKeyExists);

            return Expression.AndAlso(elementKeyDoesNotExist, noKeysStartWithTarget);
        }

        private Expression GetContainsElementKeyCheck()
            => _dictionaryVariables.GetMatchingKeyAssignment(TargetElementKey, MapperData);

        private Expression GetNoKeysWithMatchingStartQuery()
            => _dictionaryVariables.GetNoKeysWithMatchingStartQuery(TargetElementKey, MapperData);

        public Expression SourceElement { get; }

        private ParameterExpression TargetElementKey { get; }

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
        {
            if (DoNotPerformElementChecks)
            {
                return GetElementMappingBlock(GetDictionaryToElementMapping(enumerableMappingData));
            }

            var entryToElementMapping = _builder.GetElementConversion(SourceElement, enumerableMappingData);

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
            var dictionaryEntryAssignment = Expression.Assign(SourceElement, dictionaryValueAccess);

            return Expression.Block(
                new[] { (ParameterExpression)SourceElement },
                dictionaryEntryAssignment,
                elementMapping);
        }

        public Expression Adapt(LoopExpression loop)
        {
            loop = loop.InsertAssignment(Constants.BeforeLoopExitCheck, TargetElementKey, _targetMemberKey);

            var loopBody = (BlockExpression)loop.Body;

            var loopVariables = new List<ParameterExpression>(loopBody.Variables);

            if (_elementKeyExists != null)
            {
                loopVariables.Add(_elementKeyExists);
            }

            if (PerformElementChecks && !MapperData.IsRoot)
            {
                loopVariables.Add(_dictionaryVariables.Key);
            }

            loopBody = loopBody.Update(loopVariables, loopBody.Expressions);

            loop = loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);

            return loop;
        }
    }
}