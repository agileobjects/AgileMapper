namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly Expression<MapperFunc<TSource, TTarget>> _mappingLambda;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _childMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _elementMappersByKey;

        public ObjectMapper(
            Expression mappingExpression,
            ObjectMapperData mapperData)
        {
            MappingExpression = mappingExpression;
            MapperData = mapperData;

            if (mapperData.IsForStandaloneMapping)
            {
                _mappingLambda = GetStandaloneMappingLambda(mappingExpression, mapperData);
                _mapperFunc = _mappingLambda.Compile();
            }
            else
            {
                mapperData.RegisterMapperFuncIfRequired<TSource, TTarget>(mappingExpression);
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

        private static Expression<MapperFunc<TSource, TTarget>> GetStandaloneMappingLambda(
            Expression mappingExpression,
            ObjectMapperData mapperData)
        {
            var mapperFuncVariables = new List<ParameterExpression>();
            var mapperFuncAssignments = new List<Expression>();

            PopulateMapperFuncs(mapperData, mapperFuncVariables, mapperFuncAssignments);

            if (mapperFuncVariables.Any())
            {
                mapperFuncAssignments.Add(mappingExpression);
                mappingExpression = Expression.Block(mapperFuncVariables, mapperFuncAssignments);
            }

            var mappingLambda = mapperData.GetMappingLambda<TSource, TTarget>(mappingExpression);

            return mappingLambda;
        }

        private static void PopulateMapperFuncs(
            ObjectMapperData mapperData,
            ICollection<ParameterExpression> mapperFuncVariables,
            ICollection<Expression> mapperFuncAssignments)
        {
            foreach (var childMapperData in mapperData.ChildMapperDatas)
            {
                PopulateMapperFunc(childMapperData, mapperFuncVariables, mapperFuncAssignments);
                PopulateMapperFuncs(childMapperData, mapperFuncVariables, mapperFuncAssignments);
            }
        }

        private static void PopulateMapperFunc(
            ObjectMapperData mapperData,
            ICollection<ParameterExpression> mapperFuncVariables,
            ICollection<Expression> mapperFuncAssignments)
        {
            if (mapperData.HasMapperFuncVariable)
            {
                mapperFuncVariables.Add((ParameterExpression)mapperData.MapperFuncAssignment.Left);
                mapperFuncAssignments.Add(mapperData.MapperFuncAssignment);
            }
        }

        public Expression MappingExpression { get; }

        public LambdaExpression MappingLambda => _mappingLambda;

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