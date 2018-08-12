namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct MapRepeatedCallRepeatMappingStrategy : IRepeatMappingStrategy
    {
        public Expression GetMapRepeatedCallFor(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            if (DoNotMap(childMapperData))
            {
                return Constants.EmptyExpression;
            }

            childMapperData.CacheMappedObjects = true;

            childMapperData.RegisterRequiredMapperFunc(childMappingData);

            var mapRepeatedCall = declaredTypeMapperData.GetMapRepeatedCall(
                childMapperData.TargetMember,
                mappingValues,
                dataSourceIndex);

            return mapRepeatedCall;
        }

        private static bool DoNotMap(IMemberMapperData mapperData)
        {
            if (mapperData.SourceType.IsDictionary())
            {
                return true;
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
    }
}