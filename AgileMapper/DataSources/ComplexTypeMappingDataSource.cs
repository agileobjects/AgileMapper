namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
            : base(
                  bestMatchingSourceMember ?? mapperData.SourceMember,
                  GetMapping(bestMatchingSourceMember ?? mapperData.SourceMember, dataSourceIndex, mapperData))
        {
        }

        private static Expression GetMapping(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var relativeMember = sourceMember.RelativeTo(mapperData.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(mapperData.SourceObject);

            var mapping = InlineMappingFactory.GetChildMapping(
                relativeMember,
                relativeMemberAccess,
                dataSourceIndex,
                mapperData);

            return mapping;
        }
    }
}