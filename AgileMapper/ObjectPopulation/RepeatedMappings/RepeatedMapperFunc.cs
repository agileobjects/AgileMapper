namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class RepeatedMapperFunc<TChildSource, TChildTarget> : IRepeatedMapperFunc
    {
        private readonly ObjectMapperData _mapperData;
        private MapperFunc<TChildSource, TChildTarget> _repeatedMappingFunc;

        public RepeatedMapperFunc(IObjectMappingData mappingData, bool lazyLoadFuncs)
        {
            if (lazyLoadFuncs)
            {
                _mapperData = mappingData.MapperData;
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

            EnsureFunc(typedData);

            return _repeatedMappingFunc.Invoke(typedData);
        }

        private void EnsureFunc(ObjectMappingData<TChildSource, TChildTarget> mappingData)
        {
            if (_repeatedMappingFunc == null)
            {
                lock (this)
                {
                    mappingData.MapperData = _mapperData;

                    CreateMapperFunc(mappingData, isLazyLoading: true);
                }
            }
        }

        private void CreateMapperFunc(IObjectMappingData mappingData, bool isLazyLoading = false)
        {
            mappingData.MapperKey.MappingData = mappingData;
            mappingData.MapperKey.MapperData = mappingData.MapperData;

            MappingLambda = mappingData.GetOrCreateMapper().MappingLambda;

            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)MappingLambda;
            _repeatedMappingFunc = typedMappingLambda.Compile();

            if (isLazyLoading)
            {
                _mapperData.GetRootMapperData().Mapper.CacheRepeatedMappingFuncs();
            }

            mappingData.MapperKey.MapperData = null;
            mappingData.MapperKey.MappingData = null;
        }
    }
}