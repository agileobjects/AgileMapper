namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Members;

    internal class PreserveExistingValueMemberPopulationGuardFactory : IMemberPopulationGuardFactory
    {
        public static readonly IMemberPopulationGuardFactory Instance = new PreserveExistingValueMemberPopulationGuardFactory();

        public Expression GetPopulationGuardOrNull(IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsReadable)
            {
                return null;
            }

            var existingValueIsDefault = mapperData.TargetMember.GetHasDefaultValueCheck(mapperData);

            return existingValueIsDefault;
        }
    }
}