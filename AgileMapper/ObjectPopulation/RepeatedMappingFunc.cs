namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Caching;

    internal class RepeatedMappingFunc<TChildSource, TChildTarget> : IObjectMapperFunc
    {
        private readonly IRepeatedMappingFuncSet _repeatedMappingFuncs;
        private readonly ObjectMapperData _mapperData;
        private MapperFunc<TChildSource, TChildTarget> _mapperFunc;

        public RepeatedMappingFunc(
            IRepeatedMappingFuncSet repeatedMappingFuncs,
            IObjectMappingData mappingData,
            bool lazyLoadFuncs)
        {
            _repeatedMappingFuncs = repeatedMappingFuncs;

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

        public TChildTarget Map(
            ObjectMappingData<TChildSource, TChildTarget> mappingData,
            ObjectCache mappedObjectsCache)
        {
            EnsureMappingFunc(mappingData);

            return _mapperFunc.Invoke(mappingData, mappedObjectsCache, _repeatedMappingFuncs);
        }

        private void EnsureMappingFunc(IObjectMappingData mappingData)
        {
            if (_mapperFunc != null)
            {
                return;
            }

            lock (this)
            {
                mappingData.MapperData = _mapperData;

                CreateMapperFunc(mappingData);
            }
        }

        private void CreateMapperFunc(IObjectMappingData mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;
            mappingData.MapperKey.MapperData = mappingData.MapperData;

            MappingLambda = mappingData.Mapper.MappingLambda;

            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)MappingLambda;
            _mapperFunc = typedMappingLambda.Compile();

            mappingData.MapperKey.MapperData = null;
            mappingData.MapperKey.MappingData = null;
        }
    }
}
