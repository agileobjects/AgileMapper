namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Looping
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using static Constants;

    internal class IndexedSourcePopulationLoopData : IPopulationLoopData
    {
        private readonly EnumerablePopulationBuilder _builder;
        private readonly Expression _indexedSourceAccess;
        private readonly bool _useDirectValueAccess;

        public IndexedSourcePopulationLoopData(EnumerablePopulationBuilder builder)
        {
            _builder = builder;
            ContinueLoopTarget = Expression.Label(typeof(void), "Continue");
            LoopExitCheck = Expression.Equal(builder.Counter, builder.GetSourceCountAccess());

            _indexedSourceAccess = builder.GetSourceIndexAccess();

            _useDirectValueAccess =
                builder.TargetElementsAreSimple ||
                builder.SourceTypeHelper.ElementType.RuntimeTypeNeeded() ||
                builder.TargetTypeHelper.ElementType.RuntimeTypeNeeded();

            SourceElement = _useDirectValueAccess
                ? _indexedSourceAccess
                : builder.Context.GetSourceParameterFor(builder.SourceTypeHelper.ElementType);
        }

        public bool NeedsContinueTarget { get; set; }

        public LabelTarget ContinueLoopTarget { get; }

        public Expression LoopExitCheck { get; }

        public Expression SourceElement { get; }

        public Expression GetElementMapping(IObjectMappingData enumerableMappingData)
            => _builder.GetElementConversion(SourceElement, enumerableMappingData);

        public Expression Adapt(LoopExpression loop)
        {
            if (_useDirectValueAccess)
            {
                return loop;
            }

            var sourceVariable = (ParameterExpression)SourceElement;

            return loop
                .InsertAssignment(AfterLoopExitCheck, sourceVariable, _indexedSourceAccess);
        }
    }
}