namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IRecursionMapperFunc
    {
        private readonly ObjectMapperData _mapperData;
        private MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(IObjectMappingData mappingData, bool lazyLoadFuncs)
        {
            if (lazyLoadFuncs)
            {
                _mapperData = mappingData.MapperData;
                mappingData.MapperKey.MappingData = null;
                return;
            }

            CreateMapperFunc(mappingData);
        }

        public Type SourceType => typeof(TChildSource);

        public Type TargetType => typeof(TChildTarget);

        public LambdaExpression MappingLambda { get; private set; }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            EnsureRecursionFunc(typedData);

            return _recursionMapperFunc.Invoke(typedData);
        }

        private void EnsureRecursionFunc(ObjectMappingData<TChildSource, TChildTarget> mappingData)
        {
            if (_recursionMapperFunc == null)
            {
                lock (this)
                {
                    mappingData.MapperData = _mapperData;

                    CreateMapperFunc(mappingData);
                }
            }
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
    }
}