namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase
    {
        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression value,
            Expression condition,
            IMemberMappingContext context)
            : base(
                  new ConfiguredQualifiedMember(value, context),
                  GetFinalValue(dataSourceIndex, value, context),
                  context,
                  condition)

        {
        }

        private static Expression GetFinalValue(int dataSourceIndex, Expression value, IMemberMappingContext context)
        {
            if (context.TargetMember.IsComplex && (context.TargetMember.Type.Assembly != typeof(string).Assembly))
            {
                return ComplexTypeMappingDataSource.GetMapCall(value, dataSourceIndex, context);
            }

            return value;
        }
    }
}