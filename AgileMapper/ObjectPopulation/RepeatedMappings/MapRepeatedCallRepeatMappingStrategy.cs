namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
    using Members;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct MapRepeatedCallRepeatMappingStrategy : IRepeatMappingStrategy
    {
        public bool AppliesTo(IBasicMapperData mapperData) => !mapperData.TargetMember.IsEnumerable;

        public bool WillNotMap(IBasicMapperData mapperData)
        {
            if (mapperData.SourceType.IsDictionary())
            {
                return mapperData.TargetMember.Depth > 3;
            }

            while (mapperData != null)
            {
                if (mapperData.TargetMember.IsDictionary ||
                    mapperData.TargetType.Name.EndsWith("Dto", StringComparison.Ordinal) ||
                    mapperData.TargetType.Name.EndsWith("DataTransferObject", StringComparison.Ordinal))
                {
                    return true;
                }

                mapperData = mapperData.Parent;
            }

            return false;
        }

        public Expression GetMapRepeatedCallFor(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = mappingData.MapperData;

            if (WillNotMap(childMapperData))
            {
                return Constants.EmptyExpression;
            }

            childMapperData.CacheMappedObjects = true;

            childMapperData.RegisterRequiredMapperFunc(mappingData);

            var mapRepeatedCall = declaredTypeMapperData.GetMapRepeatedCall(
                childMapperData.TargetMember,
                mappingValues,
                dataSourceIndex);

            return mapRepeatedCall;
        }
    }
}