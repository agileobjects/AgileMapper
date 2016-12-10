namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class SourceElementsDictionaryPopulationLoopData : IPopulationLoopData
    {
        private static readonly MethodInfo _stringConcatMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Concat") &&
                        (m.GetParameters().Length == 3) &&
                        (m.GetParameters().First().ParameterType == typeof(string)));

        private readonly EnumerablePopulationBuilder _builder;
        private readonly Expression _targetMemberKey;

        public SourceElementsDictionaryPopulationLoopData(EnumerablePopulationBuilder builder)
            : this(GetDictionaryVariables(builder), builder)
        {
        }

        private static DictionaryEntryVariablePair GetDictionaryVariables(EnumerablePopulationBuilder builder)
        {
            var dictionarySourceMember =
                (builder.MapperData.SourceMember as DictionarySourceMember)
                ?? new DictionarySourceMember(builder.MapperData);

            return new DictionaryEntryVariablePair(dictionarySourceMember, builder.MapperData);
        }

        public SourceElementsDictionaryPopulationLoopData(
            DictionaryEntryVariablePair dictionaryVariables,
            EnumerablePopulationBuilder builder)
        {
            _builder = builder;

            var mapperData = builder.MapperData;
            _targetMemberKey = GetTargetMemberDictionaryElementKey(mapperData, builder.Counter);

            ElementKey = dictionaryVariables.Key;
            SourceElement = dictionaryVariables.GetEntryValueAccess(mapperData);

            LoopExitCheck = mapperData.IsRoot
                ? GetContainsKeyLoopExitCheck(mapperData)
                : GetKeyNotFoundLoopExitCheck(dictionaryVariables, mapperData);
        }

        private static Expression GetTargetMemberDictionaryElementKey(IMemberMapperData mapperData, Expression counter)
        {
            var name = mapperData.IsRoot ? null : mapperData.TargetMember.Name;
            var nameAndOpenBrace = Expression.Constant(name + "[");
            var counterString = mapperData.MapperContext.ValueConverters.GetConversion(counter, typeof(string));
            var closeBrace = Expression.Constant("]");

            var nameConstant = Expression.Call(
                null,
                _stringConcatMethod,
                nameAndOpenBrace,
                counterString,
                closeBrace);

            return nameConstant;
        }

        private Expression GetContainsKeyLoopExitCheck(IMemberMapperData mapperData)
        {
            var containsKeyMethod = mapperData.SourceObject.Type.GetMethod("ContainsKey");
            var containsKeyCall = Expression.Call(mapperData.SourceObject, containsKeyMethod, ElementKey);

            return Expression.Not(containsKeyCall);
        }

        private Expression GetKeyNotFoundLoopExitCheck(
            DictionaryEntryVariablePair dictionaryVariables,
            IMemberMapperData mapperData)
        {
            var keyVariableAssignment = dictionaryVariables.GetMatchingKeyAssignment(_targetMemberKey, mapperData);

            return keyVariableAssignment.GetIsDefaultComparison();
        }

        public ParameterExpression ElementKey { get; }

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
            => _builder.GetElementConversion(SourceElement, enumerableMappingData);

        public Expression Adapt(LoopExpression loop)
        {
            if (_builder.MapperData.IsRoot)
            {
                return loop.InsertAssignment(
                    Constants.BeforeLoopExitCheck,
                    ElementKey,
                    _targetMemberKey);
            }

            return Expression.Block(new[] { ElementKey }, loop);
        }

        public Expression SourceElement { get; }
    }
}