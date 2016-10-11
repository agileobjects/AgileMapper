namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper<TTarget>
    {
        private readonly Expression<MapperFunc<TSource, TTarget>> _mappingLambda;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;

        public ObjectMapper(Expression<MapperFunc<TSource, TTarget>> mappingLambda)
        {
            _mappingLambda = mappingLambda;
            _mapperFunc = mappingLambda.Compile();
        }

        public LambdaExpression MappingLambda => _mappingLambda;

        public TTarget Execute(IObjectMappingContextData data)
        {
            var typedData = (ObjectMappingContextData<TSource, TTarget>)data;

            return _mapperFunc.Invoke(typedData);
        }
    }
}