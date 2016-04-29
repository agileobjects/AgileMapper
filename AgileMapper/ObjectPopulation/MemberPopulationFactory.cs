namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<MemberPopulation> Create(IObjectMappingContext omc)
        {
            return omc.MappingContext.GlobalContext.MemberFinder
                .GetTargetMembers(omc.ExistingObject.Type)
                .Select(targetMember => Create(targetMember, omc));
        }

        private static MemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var bestMatchingDataSource = omc
                .MappingContext
                .MapperContext
                .DataSources
                .GetBestMatchFor(targetMember, omc);

            if (bestMatchingDataSource == null)
            {
                return MemberPopulation.Empty;
            }

            var value = bestMatchingDataSource.GetValue(omc);
            var convertedValue = GetConvertedValue(value, targetMember, omc);
            var population = targetMember.GetPopulation(omc.TargetVariable, convertedValue);

            return new MemberPopulation(targetMember, value, population, omc);
        }
        private static Expression GetConvertedValue(
            Expression value,
            Member targetMember,
            IObjectMappingContext omc)
        {
            var valueConversion = omc
                .MappingContext
                .MapperContext
                .ValueConverters
                .GetConversion(value, targetMember.Type);

            return valueConversion;
        }
    }
}