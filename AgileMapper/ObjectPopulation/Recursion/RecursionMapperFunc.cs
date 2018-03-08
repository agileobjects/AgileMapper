namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IRecursionMapperFunc
    {
        private MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(IObjectMappingData mappingData)
        {
            CreateMapperFunc(mappingData);
        }

        private void CreateMapperFunc(IObjectMappingData mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;
            mappingData.MapperKey.MapperData = mappingData.MapperData;

            MappingLambda = mappingData.Mapper.MappingLambda;

            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)MappingLambda;
            _recursionMapperFunc = typedMappingLambda.Compile();

            mappingData.MapperKey.MapperData = null;
            mappingData.MapperKey.MappingData = null;
        }

        public Type SourceType => typeof(TChildSource);

        public Type TargetType => typeof(TChildTarget);

        public LambdaExpression MappingLambda { get; private set; }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            return _recursionMapperFunc.Invoke(typedData);
        }
    }
}