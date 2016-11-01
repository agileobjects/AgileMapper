namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Members;

    internal class ObjectMapperFactory
    {
        private readonly EnumerableMappingExpressionFactory _enumerableMappingExpressionFactory;
        private readonly ComplexTypeMappingExpressionFactory _complexTypeMappingExpressionFactory;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _rootMappers;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _enumerableMappingExpressionFactory = new EnumerableMappingExpressionFactory();
            _complexTypeMappingExpressionFactory = new ComplexTypeMappingExpressionFactory(mapperContext);
            _rootMappers = mapperContext.Cache.CreateScoped<ObjectMapperKeyBase, IObjectMapper>();
        }

        public IObjectMapper GetOrCreateRoot(IObjectMappingData mappingData) => _rootMappers.GetOrAddMapper(mappingData);

        public IObjectMapper Create<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var mappingExpression = mappingData.MapperKey.MappingTypes.IsEnumerable
                ? _enumerableMappingExpressionFactory.Create(mappingData)
                : _complexTypeMappingExpressionFactory.Create(mappingData);

            var mapperData = mappingData.MapperData;

            Expression<MapperFunc<TSource, TTarget>> mappingLambda = null;

            if (mapperData.HasRequiredMapperFunc)
            {
                mappingLambda = CreateMappingLambda<TSource, TTarget>(mappingExpression, mapperData);
                mapperData.AddMapperFuncBody(mappingLambda);
            }

            if (mapperData.HasMapperFuncs)
            {
                mappingExpression = PrependMapperFuncsTo(mappingExpression, mapperData);
                mappingLambda = null;
            }

            if (mappingLambda == null)
            {
                mappingLambda = CreateMappingLambda<TSource, TTarget>(mappingExpression, mapperData);
            }

            var mapper = new ObjectMapper<TSource, TTarget>(mappingLambda, mapperData);

            return mapper;
        }

        private static Expression PrependMapperFuncsTo(Expression mappingExpression, ObjectMapperData mapperData)
        {
            var allMappingExpressions = mapperData
                .RequiredMapperFuncsByVariable
                .Select(kvp => (Expression)Expression.Assign(kvp.Key, kvp.Value))
                .ToList();

            allMappingExpressions.Add(mappingExpression);

            var updatedMappingExpression = Expression.Block(
                mapperData.RequiredMapperFuncsByVariable.Keys,
                allMappingExpressions);

            return updatedMappingExpression;
        }

        private static Expression<MapperFunc<TSource, TTarget>> CreateMappingLambda<TSource, TTarget>(
            Expression mappingExpression,
            IMemberMapperData mapperData)
        {
            return Expression.Lambda<MapperFunc<TSource, TTarget>>(mappingExpression, mapperData.MappingDataObject);
        }

        public void Reset()
        {
            _rootMappers.Empty();
            _complexTypeMappingExpressionFactory.Reset();
        }
    }
}