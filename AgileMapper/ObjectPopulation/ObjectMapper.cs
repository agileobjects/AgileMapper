namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper<TTarget>
    {
        private readonly MappingRuleSet _ruleSet;
        private readonly Expression<MapperFunc<TSource, TTarget>> _mapperLambda;
        private readonly Lazy<ParameterExpression> _mapperVariableLoader;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;

        public ObjectMapper(
            MappingRuleSet ruleSet,
            Expression<MapperFunc<TSource, TTarget>> mapperLambda)
        {
            _ruleSet = ruleSet;
            _mapperLambda = mapperLambda;
            _mapperVariableLoader = new Lazy<ParameterExpression>(CreateMapperVariable, isThreadSafe: true);

            _mapperFunc = mapperLambda.Compile();
        }

        private ParameterExpression CreateMapperVariable()
        {
            var sourceTypeName = typeof(TSource).GetVariableName(f => f.InPascalCase);
            var targetTypeName = typeof(TSource).GetVariableName(f => f.InPascalCase);
            var mapperVariableName = "map" + sourceTypeName + "To" + targetTypeName + _ruleSet.Name;

            return Expression.Variable(_mapperLambda.Type, mapperVariableName);
        }

        public ParameterExpression MapperVariable => _mapperVariableLoader.Value;

        public LambdaExpression MapperLambda => _mapperLambda;

        public TTarget Execute(IObjectMapperCreationData data)
        {
            var typedData = (ObjectMappingData<TSource, TTarget>)data;

            return _mapperFunc.Invoke(typedData);
        }
    }
}