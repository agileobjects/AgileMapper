namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper<TTarget>
    {
        private readonly Expression<MapperFunc<TSource, TTarget>> _mapperLambda;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;

        public ObjectMapper(Expression<MapperFunc<TSource, TTarget>> mapperLambda)
        {
            _mapperLambda = mapperLambda;
            _mapperFunc = mapperLambda.Compile();
        }

        public TTarget Execute(IObjectMappingContext objectMappingContext)
        {
            var typedObjectMappingContext =
                (ObjectMappingContext<TSource, TTarget>)objectMappingContext;

            return _mapperFunc.Invoke(typedObjectMappingContext);
        }
    }
}