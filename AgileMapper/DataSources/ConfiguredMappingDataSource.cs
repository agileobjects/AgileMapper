namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using Members.Extensions;
    using ObjectPopulation;

    internal static class ConfiguredMappingDataSource
    {
        public static IDataSource Create(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;

            if (!childMapperData.MapperContext.UserConfigurations.HasMappingFactories)
            {
                return NullDataSource.EmptyValue;
            }

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMappingData.Parent);

            var childCreationContext = new MappingCreationContext(childObjectMappingData);

            var mapping = ConfiguredMappingFactory
                .GetMappingOrNull(childCreationContext, out _);

            if (mapping == null)
            {
                return NullDataSource.EmptyValue;
            }

            var childObjectMapperData = childObjectMappingData.MapperData;
            var sourceMemberAccess = sourceMember.GetRelativeQualifiedAccess(childMapperData);
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.ElementIndex,
                childMapperData.ElementKey,
                dataSourceIndex);

            var directAccessMapping = MappingFactory.GetDirectAccessMapping(
                mapping,
                childObjectMapperData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, childObjectMapperData));

            var returnValue = childObjectMapperData
                .GetFinalisedReturnValue(directAccessMapping, out var returnsDefault);

            if (returnsDefault)
            {
                returnValue = Expression.Block(directAccessMapping, returnValue);
            }

            return new AdHocDataSource(sourceMember, returnValue, childMapperData);
        }
    }
}