namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System.Linq.Expressions;
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

            childMapperData.CacheMappedObjects = !childMapperData.SourceIsFlatObject();

            childMapperData.RegisterRequiredMapperFunc(childMappingData);

            var mapRecursionCall = declaredTypeMapperData.GetMapRecursionCall(
                sourceValue,
                childMapperData.TargetMember,
                dataSourceIndex);

            return mapRecursionCall;
        }
    }
}