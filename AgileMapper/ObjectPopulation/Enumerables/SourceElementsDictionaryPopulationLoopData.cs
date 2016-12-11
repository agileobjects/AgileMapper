namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
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

            var mapperData = builder.MapperData;

            _targetMemberKey = dictionaryVariables
                .GetTargetMemberDictionaryEnumerableElementKey(mapperData, builder.Counter);

            _useDirectValueAccess = builder.ElementTypesAreSimple
                ? builder.Context.ElementTypesAreAssignable || (builder.Context.TargetElementType == typeof(string))
                : dictionaryVariables.Value.Type == typeof(object);

            SourceElement = _useDirectValueAccess ? GetDictionaryEntryValueAccess() : dictionaryVariables.Value;

            LoopExitCheck = mapperData.IsRoot
                ? GetRootLoopExitCheck(builder)
                : GetKeyNotFoundLoopExitCheck(dictionaryVariables, mapperData);
        }

        private Expression GetDictionaryEntryValueAccess()
            => _dictionaryVariables.GetEntryValueAccess(_builder.MapperData);

        private Expression GetRootLoopExitCheck(EnumerablePopulationBuilder builder)
        {
            var containsElementKeyCall = GetContainsElementKeyCall(builder.MapperData);

            if (builder.Context.ElementTypesAreSimple)
            {
                return Expression.Not(containsElementKeyCall);
            }

            _elementKeyExists = Expression.Variable(typeof(bool), "elementKeyExists");
            var keyExistsAssignment = Expression.Assign(_elementKeyExists, containsElementKeyCall);
            var elementKeyDoesNotExist = Expression.Not(keyExistsAssignment);

            var noKeysStartWithTarget = _dictionaryVariables.GetNoKeysWithMatchingStartQuery(builder.MapperData);

            return Expression.AndAlso(elementKeyDoesNotExist, noKeysStartWithTarget);
        }

        private Expression GetContainsElementKeyCall(IMemberMapperData mapperData)
        {
            var containsKeyMethod = mapperData.SourceObject.Type.GetMethod("ContainsKey");
            var containsKeyCall = Expression.Call(mapperData.SourceObject, containsKeyMethod, ElementKey);

            return containsKeyCall;
        }

        private Expression GetKeyNotFoundLoopExitCheck(
            DictionaryEntryVariablePair dictionaryVariables,
            IMemberMapperData mapperData)
        {
            var keyVariableAssignment = dictionaryVariables.GetMatchingKeyAssignment(ElementKey, mapperData);

            return keyVariableAssignment.GetIsDefaultComparison();
        }

        public Expression SourceElement { get; }

        public ParameterExpression ElementKey => _dictionaryVariables.Key;

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
        {
            var entryToElementMapping = _builder.GetElementConversion(SourceElement, enumerableMappingData);

            if (_useDirectValueAccess && _builder.Context.ElementTypesAreSimple)
            {
                return entryToElementMapping;
            }

            if (ContainsElementKeyCheckNotPerformed)
            {
                return GetElementMappingBlock(entryToElementMapping);
            }

            var elementMappingData = ObjectMappingDataFactory.ForElement(
                _builder.MapperData.SourceType,
                entryToElementMapping.Type,
                enumerableMappingData);

            var dictionaryToElementMapping = MappingFactory.GetElementMapping(
                _builder.MapperData.SourceObject,
                Expression.Default(_builder.Context.TargetElementType),
                elementMappingData);

            var mapElementOrElementMembers = Expression.Condition(
                _elementKeyExists,
                entryToElementMapping,
                dictionaryToElementMapping);

            return GetElementMappingBlock(mapElementOrElementMembers);
        }

        private bool ContainsElementKeyCheckNotPerformed => _elementKeyExists == null;

        private Expression GetElementMappingBlock(Expression elementMapping)
        {
            if (_useDirectValueAccess)
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
            loop = loop.InsertAssignment(Constants.BeforeLoopExitCheck, ElementKey, _targetMemberKey);

            if (_elementKeyExists == null)
            {
                return loop;
            }

            var loopBody = (BlockExpression)loop.Body;

            loopBody = loopBody.Update(loopBody.Variables.Concat(_elementKeyExists), loopBody.Expressions);

            loop = loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);

            return loop;
        }
    }
}