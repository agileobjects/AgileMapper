namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

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

            var mappingValues = new MappingValues(
                sourceValue,
                childMapperData.TargetMember.GetAccess(declaredTypeMapperData),
                declaredTypeMapperData.EnumerableIndex);

            var createMappingDataCall = MappingDataCreationFactory.ForChild(
                mappingValues,
                dataSourceIndex,
                childMapperData);

            var performRepeatedMappingMethod = typeof(IRepeatedMappingFuncSet)
                .GetPublicInstanceMethod(nameof(IRepeatedMappingFuncSet.Map))
                .MakeGenericMethod(childMapperData.SourceType, childMapperData.TargetType);

            var performRepeatedMappingCall = Expression.Call(
                Parameters.RepeatedMappingFuncs,
                performRepeatedMappingMethod,
                createMappingDataCall,
                Parameters.MappedObjectsCache);

            return performRepeatedMappingCall;
        }

        private static bool DoNotMapRecursion(IMemberMapperData mapperData)
        {
            if (mapperData.SourceType.IsDictionary())
            {
                return true;
            }

            while (mapperData != null)
            {
                if (mapperData.TargetType.Name.EndsWith("Dto", StringComparison.Ordinal) ||
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