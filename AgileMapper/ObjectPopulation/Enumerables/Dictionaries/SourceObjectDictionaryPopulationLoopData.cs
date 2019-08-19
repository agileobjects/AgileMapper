namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions.Internal;

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
            _sourceEnumerableFound = Parameters.Create<bool>("sourceEnumerableFound");

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
            return Expression.Condition(
                _sourceEnumerableFound,
                _enumerableLoopData.SourceElement,
                _elementsDictionaryLoopData.GetElementMapping(enumerableMappingData));
        }

        public Expression Adapt(LoopExpression loop)
        {
            var sourceEnumerableFoundTest = GetSourceEnumerableFoundTest(_emptyTarget, _builder);
            var assignSourceEnumerableFound = (Expression)_sourceEnumerableFound.AssignTo(sourceEnumerableFoundTest);

            var adaptedLoop = _elementsDictionaryLoopData.Adapt(loop);

            var enumerableLoopBlock = _enumerableLoopData.GetLoopBlock(
                adaptedLoop,
                GetEnumeratorIfNecessary,
                DisposeEnumeratorIfNecessary);

            return Expression.Block(
                enumerableLoopBlock.Variables.Append(_sourceEnumerableFound),
                enumerableLoopBlock.Expressions.Prepend(assignSourceEnumerableFound));
        }

        public static BinaryExpression GetSourceEnumerableFoundTest(
            Expression emptyTarget,
            EnumerablePopulationBuilder builder)
        {
            return Expression.NotEqual(builder.SourceValue, emptyTarget);
        }

        private Expression GetEnumeratorIfNecessary(Expression getEnumeratorCall)
            => getEnumeratorCall.ToIfFalseDefaultCondition(_sourceEnumerableFound);

        private Expression DisposeEnumeratorIfNecessary(Expression disposeEnumeratorCall)
            => Expression.IfThen(_sourceEnumerableFound, disposeEnumeratorCall);
    }
}