namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IRecursionMapperFunc
    {
        private readonly MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(LambdaExpression mappingLambda)
        {
            MappingLambda = mappingLambda;

            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)mappingLambda;
            _recursionMapperFunc = typedMappingLambda.Compile();
        }

        public Type SourceType => typeof(TChildSource);

        public Type TargetType => typeof(TChildTarget);

        public LambdaExpression MappingLambda { get; }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            return _recursionMapperFunc.Invoke(typedData);
        }
    }
}