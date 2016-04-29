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
            var qualifiedTargetMember = omc.TargetMember.Append(targetMember);

            var bestMatchingDataSource = omc
                .MappingContext
                .MapperContext
                .DataSources
                .GetBestMatchFor(qualifiedTargetMember, omc);

            if (bestMatchingDataSource == null)
            {
                return MemberPopulation.Empty;
            }

            var value = bestMatchingDataSource.GetValue(omc);
            var convertedValue = GetConvertedValue(value, qualifiedTargetMember, omc);
            var population = targetMember.GetPopulation(omc.TargetVariable, convertedValue);

            return new MemberPopulation(qualifiedTargetMember, value, population, omc);
        }
        private static Expression GetConvertedValue(
            Expression value,
            QualifiedMember qualifiedTargetMember,
            IObjectMappingContext omc)
        {
            var valueConversion = omc
                .MappingContext
                .MapperContext
                .ValueConverters
                .GetConversion(value, qualifiedTargetMember.Type);

            return valueConversion;
        }
    }
}