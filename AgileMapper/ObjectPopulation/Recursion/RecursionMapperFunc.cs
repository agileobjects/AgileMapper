namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IRecursionMapperFunc
    {
        private readonly ObjectMapperData _mapperData;
        private MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(IObjectMappingData mappingData, bool lazyLoadFuncs)
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
            _recursionMapperFunc = typedMappingLambda.Compile();

            if (isLazyLoading)
            {
                _mapperData.GetRootMapperData().Mapper.CacheRecursionMapperFuncs();
            }

            mappingData.MapperKey.MapperData = null;
            mappingData.MapperKey.MappingData = null;
        }
    }
}