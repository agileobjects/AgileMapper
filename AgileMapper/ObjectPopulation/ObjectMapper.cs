namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class ObjectMapper<TSource, TTarget, TInstance> : IObjectMapper<TInstance>
    {
        private readonly Expression<MapperFunc<TSource, TTarget, TInstance>> _mappingLambda;
        private readonly MapperFunc<TSource, TTarget, TInstance> _mapperFunc;

        public ObjectMapper(Expression<MapperFunc<TSource, TTarget, TInstance>> mappingLambda)
        {
            _mappingLambda = mappingLambda;
            _mapperFunc = mappingLambda.Compile();
        }

        public LambdaExpression MappingLambda => _mappingLambda;

        public TInstance Execute(IObjectMappingContext objectMappingContext)
        {
            var typedObjectMappingContext =
                (ObjectMappingContext<TSource, TTarget, TInstance>)objectMappingContext;

            return _mapperFunc.Invoke(typedObjectMappingContext);
        }
    }
}