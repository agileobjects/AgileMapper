namespace AgileObjects.AgileMapper.Members
{
    using DataSources;

    internal static class MemberMappingContextExtensions
    {
        public static DataSourceSet GetDataSources(this IMemberMappingContext context)
            => context.MappingContext.MapperContext.DataSources.FindFor(context);

        public static IDataSource DataSourceAt(this IMemberMappingContext context, int index)
            => context.MappingContext.MapperContext.DataSources.DataSourceAt(index, context);
    }
}