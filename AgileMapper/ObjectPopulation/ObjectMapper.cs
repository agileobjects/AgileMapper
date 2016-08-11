namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper<TSource, TTarget>
    {
        private readonly Expression<MapperFunc<TSource, TTarget>> _mappingLambda;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;

        public ObjectMapper(Expression<MapperFunc<TSource, TTarget>> mappingLambda)
        {
            _mappingLambda = mappingLambda;
            _mapperFunc = mappingLambda.Compile();
        }

        public LambdaExpression MappingLambda => _mappingLambda;

        public TTarget Execute(MappingData<TSource, TTarget> data)
        {
            return _mapperFunc.Invoke(data);
        }
    }
}