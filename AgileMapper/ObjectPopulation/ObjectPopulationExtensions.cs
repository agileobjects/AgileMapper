namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal static class ObjectPopulationExtensions
    {
        public static Expression GetPopulation(this IObjectMappingContext omc, Member targetMember, Expression value)
            => targetMember.GetPopulation(omc.TargetVariable, value);
    }
}