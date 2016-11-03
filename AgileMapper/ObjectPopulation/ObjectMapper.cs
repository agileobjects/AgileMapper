namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _childMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _elementMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapperFunc> _recursionMappingFuncsByKey;

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            ObjectMapperData mapperData)
        {
            MappingLambda = mappingLambda;
            MapperData = mapperData;

            if (mapperData.IsForStandaloneMapping && !mapperData.IsPartOfDerivedTypeMapping)
            {
                _mapperFunc = mappingLambda.Compile();
            }

            if (mapperData.RequiresChildMapping)
            {
                _childMappersByKey = mapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }
            else if (mapperData.RequiresElementMapping)
            {
                _elementMappersByKey = mapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }

            if (mapperData.HasMapperFuncs)
            {
                _recursionMappingFuncsByKey = CreateRecursionMapperFuncs();
            }
        }

        private ICache<ObjectMapperKeyBase, IObjectMapperFunc> CreateRecursionMapperFuncs()
        {
            var cache = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapperFunc>();

            foreach (var mappingLambdaAndKey in MapperData.RequiredMapperFuncsByKey)
            {
                var mappingLambda = mappingLambdaAndKey.Value;
                var mappingDataObject = mappingLambda.Parameters.First();
                var mappingDataTypes = mappingDataObject.Type.GetGenericArguments();

                var typesKey = new SourceAndTargetTypesKey(mappingDataTypes[0], mappingDataTypes[1]);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RecursionMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var lambdaParameter = Parameters.Create<LambdaExpression>("lambda");

                    var mapperFuncCreation = Expression.New(mapperFuncType.GetConstructors()[0], lambdaParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<LambdaExpression, IObjectMapperFunc>>(
                        mapperFuncCreation,
                        lambdaParameter);

                    return mapperCreationLambda.Compile();
                });

                var mapperFunc = mapperFuncCreator.Invoke(mappingLambda);

                cache.GetOrAdd(mappingLambdaAndKey.Key, k => mapperFunc);
            }

            return cache;
        }

        #region Setup

        #endregion

        public LambdaExpression MappingLambda { get; }

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

        public object MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingData parentMappingData)
        {
            var childMappingData = MappingDataFactory.ForChild(
                source,
                target,
                enumerableIndex,
                targetMemberName,
                dataSourceIndex,
                parentMappingData);

            var mapperFunc = _recursionMappingFuncsByKey
                .GetOrAdd(childMappingData.MapperKey, null);

            return mapperFunc.Map(childMappingData);
        }

        private static object Map(
            IObjectMappingData mappingData,
            ICache<ObjectMapperKeyBase, IObjectMapper> subMapperCache)
        {
            mappingData.Mapper = subMapperCache.GetOrAddMapper(mappingData);

            return mappingData.Mapper.Map(mappingData);
        }
    }

    internal class RecursionMapperFunc<TChildSource, TChildTarget> : IObjectMapperFunc
    {
        private readonly MapperFunc<TChildSource, TChildTarget> _recursionMapperFunc;

        public RecursionMapperFunc(LambdaExpression mappingLambda)
        {
            var typedMappingLambda = (Expression<MapperFunc<TChildSource, TChildTarget>>)mappingLambda;
            _recursionMapperFunc = typedMappingLambda.Compile();
        }

        public object Map(IObjectMappingData mappingData)
        {
            var typedData = (ObjectMappingData<TChildSource, TChildTarget>)mappingData;

            return _recursionMapperFunc.Invoke(typedData);
        }
    }
}