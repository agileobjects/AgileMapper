namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;

    internal class MapRecursionCallRecursiveMemberMappingStrategy : IRecursiveMemberMappingStrategy
    {
        public static IRecursiveMemberMappingStrategy Instance = new MapRecursionCallRecursiveMemberMappingStrategy();

        public Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            if (DoNotMapRecursion(childMapperData))
            {
                return Constants.EmptyExpression;
            }

            childMapperData.CacheMappedObjects = true;

            childMapperData.RegisterRequiredMapperFunc(childMappingData);

            var mapRecursionCall = declaredTypeMapperData.GetMapRecursionCall(
                sourceValue,
                childMapperData.TargetMember,
                dataSourceIndex);

            return mapRecursionCall;
        }

        private static bool DoNotMapRecursion(IMemberMapperData mapperData)
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