namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;

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

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
        {
            var convertedEnumeratorValue = _builder
                .GetElementConversion(_enumerableLoopData.SourceElement, enumerableMappingData);

            var convertedElementValue = _builder
                .GetElementConversion(_elementsDictionaryLoopData.SourceElement, enumerableMappingData);

            return Expression.Condition(_sourceEnumerableFound, convertedEnumeratorValue, convertedElementValue);
        }

        public Expression Adapt(LoopExpression loop)
        {
            var sourceEnumerableFoundTest = Expression.NotEqual(_builder.SourceVariable, _emptyTarget);
            var assignSourceEnumerableFound = Expression.Assign(_sourceEnumerableFound, sourceEnumerableFoundTest);

            var enumerableLoopBlock = _enumerableLoopData.GetLoopBlock(
                loop,
                GetEnumeratorIfNecessary,
                DisposeEnumeratorIfNecessary);

            var blockVariables = new List<ParameterExpression>(enumerableLoopBlock.Variables)
            {
                _sourceEnumerableFound,
                _elementsDictionaryLoopData.ElementKey
            };

            return Expression.Block(
                blockVariables,
                new[] { assignSourceEnumerableFound }.Concat(enumerableLoopBlock.Expressions));
        }

        private Expression GetEnumeratorIfNecessary(Expression getEnumeratorCall)
        {
            return Expression.Condition(
                _sourceEnumerableFound,
                getEnumeratorCall,
                Expression.Default(getEnumeratorCall.Type));
        }

        private Expression DisposeEnumeratorIfNecessary(Expression disposeEnumeratorCall)
            => Expression.IfThen(_sourceEnumerableFound, disposeEnumeratorCall);
    }
}