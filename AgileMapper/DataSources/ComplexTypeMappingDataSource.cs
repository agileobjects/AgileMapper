namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            MemberMapperData data)
            : base(
                  bestMatchingSourceMember ?? data.SourceMember,
                  GetMapCall(bestMatchingSourceMember ?? data.SourceMember, dataSourceIndex, data))
        {
        }

        private static Expression GetMapCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            MemberMapperData data)
        {
            var relativeMember = sourceMember.RelativeTo(data.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(data.SourceObject);

            if (data.TargetMember.Type.IsSealed)
            {
                //return GetInlineMapperCall(relativeMember, dataSourceIndex, context);
            }

            return data.GetMapCall(relativeMemberAccess, dataSourceIndex);
        }

        private static Expression GetInlineMapperCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            MemberMapperData data)
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