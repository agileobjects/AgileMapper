namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using DataSources;

    internal static class MemberMappingContextExtensions
    {
        public static IEnumerable<IDataSource> GetDataSources(this IMemberMappingContext context)
            => context.MappingContext.MapperContext.DataSources.FindFor(context);

        public static IEnumerable<IDataSource> EnumerateDataSources(this IMemberMappingContext context)
            => context.MappingContext.MapperContext.DataSources.EnumerateDataSources(context);
    }
}