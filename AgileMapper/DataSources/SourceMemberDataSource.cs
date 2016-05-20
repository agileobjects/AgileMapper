namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(IQualifiedMember sourceMember, IMemberMappingContext context)
            : base(sourceMember, GetValueFrom(sourceMember, context), context)
        {
        }

        private static Expression GetValueFrom(IQualifiedMember sourceMember, IMemberMappingContext context)
        {
            var value = sourceMember.GetQualifiedAccess(context.SourceObject);
            var convertedValue = context.MapperContext.ValueConverters.GetConversion(value, context.TargetMember.Type);

            return convertedValue;
        }
    }
}