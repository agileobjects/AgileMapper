namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class RepeatedMapperFunc<TChildSource, TChildTarget> : IRepeatedMapperFunc
    {
        private readonly object _mappingFuncLock;
        private readonly ObjectMapperData _mapperData;
        private MapperFunc<TChildSource, TChildTarget> _repeatedMappingFunc;

        public RepeatedMapperFunc(IObjectMappingData mappingData, bool lazyLoadFuncs)
        {
            if (lazyLoadFuncs)
            {
                _mappingFuncLock = new object();
                _mapperData = mappingData.MapperData;
                _mapperData.SetEntryPoint();
                return;
            }

            CreateMapperFunc(mappingData);
        }

        public Expression Mapping { get; private set; }

        public Type SourceType => typeof(TChildSource);

        public Type TargetType => typeof(TChildTarget);

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            EnsureFunc(typedData);

            return _repeatedMappingFunc.Invoke(typedData);
        }

        private void EnsureFunc(ObjectMappingData<TChildSource, TChildTarget> mappingData)
        {
            if (_repeatedMappingFunc != null)
            {
                return;
            }

            lock (_mappingFuncLock)
            {
                if (_repeatedMappingFunc != null)
                {
                    return;
                }

                mappingData.MapperData = _mapperData;

                CreateMapperFunc(mappingData, isLazyLoading: true);
            }
        }

        private void CreateMapperFunc(IObjectMappingData mappingData, bool isLazyLoading = false)
        {
            mappingData.MapperKey.MappingData = mappingData;
            mappingData.MapperKey.MapperData = mappingData.MapperData;

            var mappingLambda = Expression.Lambda<MapperFunc<TChildSource, TChildTarget>>(
                mappingData.GetOrCreateMapper().Mapping,
                mappingData.MapperData.MappingDataObject);

            Mapping = mappingLambda;

            _repeatedMappingFunc = mappingLambda.Compile();

            if (isLazyLoading)
            {
                _mapperData.GetRootMapperData().Mapper.CacheRepeatedMappingFuncs();
            }

            mappingData.MapperKey.MapperData = null;
            mappingData.MapperKey.MappingData = null;
        }
    }
}