namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapperFunc
    {
        LambdaExpression MappingLambda { get; }
    }

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

        public LambdaExpression MappingLambda { get; private set; }

        public TChildTarget Map(ObjectMappingData<TChildSource, TChildTarget> mappingData)
        {
            EnsureRecursionFunc(mappingData);

            return _mapperFunc.Invoke(mappingData, _repeatedMappingFuncs);
        }

        private void EnsureRecursionFunc(IObjectMappingData mappingData)
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
