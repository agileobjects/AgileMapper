namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IObjectMapperFunc
    {
        private readonly MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(LambdaExpression mappingLambda)
        {
            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)mappingLambda;
            _recursionMapperFunc = typedMappingLambda.Compile();
        }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            return _recursionMapperFunc.Invoke(typedData);
        }
    }
}