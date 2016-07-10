namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using DataSources;

    internal static class MemberMappingContextExtensions
    {
        public static DataSourceSet GetDataSources(this IMemberMappingContext context)
            => context.MappingContext.MapperContext.DataSources.FindFor(context);

        public static IDataSource DataSourceAt(this IMemberMappingContext context, int index)
            => context.MappingContext.MapperContext.DataSources.FindFor(context)[index];

        public static Expression GetMapCall(this IMemberMappingContext context, Expression value, int dataSourceIndex = 0)
            => context.Parent.GetMapCall(value, context.TargetMember, dataSourceIndex);

        public static Expression[] GetNestedAccessesIn(this IMemberMappingContext context, Expression value)
        {
            return context.NestedAccessFinder.FindIn(
                value,
                context.MappingContext.RuleSet.ComplexTypeMappingShortCircuitStrategy.SourceCanBeNull);
        }
    }
}