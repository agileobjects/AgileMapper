namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class ObjectMapper<TSource, TTarget, TInstance> : IObjectMapper<TInstance>
    {
        private readonly Expression<MapperFunc<TSource, TTarget, TInstance>> _mapperLambda;
        private readonly MapperFunc<TSource, TTarget, TInstance> _mapperFunc;

        public ObjectMapper(Expression<MapperFunc<TSource, TTarget, TInstance>> mapperLambda)
        {
            _mapperLambda = mapperLambda;
            _mapperFunc = mapperLambda.Compile();
        }

        public TInstance Execute(IObjectMappingContext objectMappingContext)
        {
            var typedObjectMappingContext =
                (ObjectMappingContext<TSource, TTarget, TInstance>)objectMappingContext;

            return _mapperFunc.Invoke(typedObjectMappingContext);
        }
    }
}