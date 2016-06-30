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

        public TTarget Execute(IObjectMappingContext objectMappingContext)
        {
            var typedObjectMappingContext =
                (ObjectMappingContext<TSource, TTarget>)objectMappingContext;

            return _mapperFunc.Invoke(typedObjectMappingContext);
        }
    }
}