namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
            : base(
                  bestMatchingSourceMember ?? mapperData.SourceMember,
                  GetMapCall(bestMatchingSourceMember ?? mapperData.SourceMember, dataSourceIndex, mapperData))
        {
        }

        private static Expression GetMapCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var relativeMember = sourceMember.RelativeTo(mapperData.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(mapperData.SourceObject);

            if (mapperData.TargetType.IsSealed())
            {
                //return GetInlineMapperCall(relativeMember, dataSourceIndex, context);
            }

            return mapperData.GetMapCall(relativeMemberAccess, dataSourceIndex);
        }

        private static Expression GetInlineMapperCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            //var omcBridge = data.Parent.CreateChildMapperDataBridge(
            //    sourceMember.Type,
            //    data.TargetMember.Type,
            //    data.TargetMember.Name,
            //    dataSourceIndex);

            //var childOmc = omcBridge.ToMapperData();

            return null;
        }
    }
}