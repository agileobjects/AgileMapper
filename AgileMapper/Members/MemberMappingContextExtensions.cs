namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;

    internal static class MemberMappingContextExtensions
    {
        public static Expression GetMapCall(this MemberMapperData data, Expression value, int dataSourceIndex = 0)
            => data.Parent.GetMapCall(value, data.TargetMember, dataSourceIndex);

        public static Expression[] GetNestedAccessesIn(this MemberMapperData data, Expression value)
        {
            return data.NestedAccessFinder.FindIn(
                value,
                data.RuleSet.ComplexTypeMappingShortCircuitStrategy.SourceCanBeNull);
        }
    }
}