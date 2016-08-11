namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(IQualifiedMember sourceMember, MemberMapperData data)
            : base(sourceMember, GetValue(sourceMember, data), data)
        {
        }

        private static Expression GetValue(IQualifiedMember sourceMember, MemberMapperData data)
        {
            var value = sourceMember.GetQualifiedAccess(data.SourceObject);
            var convertedValue = data.MapperContext.ValueConverters.GetConversion(value, data.TargetMember.Type);

            return convertedValue;
        }
    }
}