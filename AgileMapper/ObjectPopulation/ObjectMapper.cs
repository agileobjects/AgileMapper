namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Caching;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly Expression<MapperFunc<TSource, TTarget>> _mappingLambda;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _childMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _elementMappersByKey;

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            ObjectMapperData mapperData)
        {
            _mappingLambda = mappingLambda;
            MapperData = mapperData;

            if (mapperData.IsForStandaloneMapping && !mapperData.IsPartOfDerivedTypeMapping)
            {
                _mapperFunc = _mappingLambda.Compile();
            }
            else
            {
                mapperData.RegisterMapperFuncIfRequired(_mappingLambda);
            }

            if (mapperData.RequiresChildMapping)
            {
                _childMappersByKey = mapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }
            else if (mapperData.RequiresElementMapping)
            {
                _elementMappersByKey = mapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }
        }

        public LambdaExpression MappingLambda => _mappingLambda;

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TSource, TTarget>)mappingData;

            return _mapperFunc.Invoke(typedData);
        }

        public object MapChild<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingData parentMappingData)
        {
            var childMappingData = ObjectMappingDataFactory.ForChild(
                source,
                target,
                enumerableIndex,
                targetMemberName,
                dataSourceIndex,
                parentMappingData);

            return Map(childMappingData, _childMappersByKey);
        }

        public object MapElement<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget targetElement,
            int? enumerableIndex,
            IObjectMappingData parentMappingData)
        {
            var elementMappingData = ObjectMappingDataFactory.ForElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                parentMappingData);

            return Map(elementMappingData, _elementMappersByKey);
        }

        private static object Map(
            IObjectMappingData mappingData,
            ICache<ObjectMapperKeyBase, IObjectMapper> subMapperCache)
        {
            mappingData.Mapper = subMapperCache.GetOrAddMapper(mappingData);

            return mappingData.Mapper.Map(mappingData);
        }
    }
}