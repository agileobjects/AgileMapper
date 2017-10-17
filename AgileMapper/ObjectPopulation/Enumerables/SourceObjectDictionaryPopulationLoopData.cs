namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class SourceObjectDictionaryPopulationLoopData : IPopulationLoopData
    {
        private readonly Expression _emptyTarget;
        private readonly EnumerablePopulationBuilder _builder;
        private readonly EnumerableSourcePopulationLoopData _enumerableLoopData;
        private readonly SourceElementsDictionaryPopulationLoopData _elementsDictionaryLoopData;
        private readonly ParameterExpression _sourceEnumerableFound;

        public SourceObjectDictionaryPopulationLoopData(
            Expression emptyTarget,
            DictionaryEntryVariablePair dictionaryVariables,
            EnumerablePopulationBuilder builder)
        {
            _emptyTarget = emptyTarget;
            _builder = builder;

            _enumerableLoopData = new EnumerableSourcePopulationLoopData(builder);
            _elementsDictionaryLoopData = new SourceElementsDictionaryPopulationLoopData(dictionaryVariables, builder);
            _sourceEnumerableFound = Expression.Variable(typeof(bool), "sourceEnumerableFound");

            ContinueLoopTarget = Expression.Label(typeof(void), "Continue");
            LoopExitCheck = GetCompositeLoopExitCheck();
        }

        private Expression GetCompositeLoopExitCheck()
        {
            var returnLabel = Expression.Label(typeof(bool), "Return");

            var returnEnumerableResult = Expression.Return(returnLabel, _enumerableLoopData.LoopExitCheck);
            var ifEnumeratorReturnResult = Expression.IfThen(_sourceEnumerableFound, returnEnumerableResult);

            var returnElementsResult = Expression.Label(returnLabel, _elementsDictionaryLoopData.LoopExitCheck);

            return Expression.Block(ifEnumeratorReturnResult, returnElementsResult);
        }

        public bool NeedsContinueTarget { get; set; }

        public LabelTarget ContinueLoopTarget { get; }

        public Expression LoopExitCheck { get; }

        public Expression GetSourceElementValue() => _elementsDictionaryLoopData.GetSourceElementValue();

        public Expression GetElementMapping(IObjectMappingData enumerableMappingData)
        {
            var convertedEnumeratorValue = _enumerableLoopData.GetElementMapping(enumerableMappingData);
            var convertedElementValue = _elementsDictionaryLoopData.GetElementMapping(enumerableMappingData);

            return Expression.Condition(_sourceEnumerableFound, convertedEnumeratorValue, convertedElementValue);
        }

        public Expression Adapt(LoopExpression loop)
        {
            var sourceEnumerableFoundTest = Expression.NotEqual(_builder.SourceValue, _emptyTarget);
            var assignSourceEnumerableFound = (Expression)_sourceEnumerableFound.AssignTo(sourceEnumerableFoundTest);

            var adaptedLoop = _elementsDictionaryLoopData.Adapt(loop);

            var enumerableLoopBlock = _enumerableLoopData.GetLoopBlock(
                adaptedLoop,
                GetEnumeratorIfNecessary,
                DisposeEnumeratorIfNecessary);

            return Expression.Block(
                new[] { _sourceEnumerableFound }.Append(enumerableLoopBlock.Variables),
                new[] { assignSourceEnumerableFound }.Append(enumerableLoopBlock.Expressions));
        }

        private Expression GetEnumeratorIfNecessary(Expression getEnumeratorCall)
        {
            return Expression.Condition(
                _sourceEnumerableFound,
                getEnumeratorCall,
                getEnumeratorCall.Type.ToDefaultExpression());
        }

        private Expression DisposeEnumeratorIfNecessary(Expression disposeEnumeratorCall)
            => Expression.IfThen(_sourceEnumerableFound, disposeEnumeratorCall);
    }
}