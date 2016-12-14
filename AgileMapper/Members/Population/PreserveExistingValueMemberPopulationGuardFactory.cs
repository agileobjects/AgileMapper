namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class PreserveExistingValueMemberPopulationGuardFactory : IMemberPopulationGuardFactory
    {
        public static readonly IMemberPopulationGuardFactory Instance = new PreserveExistingValueMemberPopulationGuardFactory();

        public Expression GetPopulationGuardOrNull(IMemberMapperData mapperData)
        {
            return mapperData.TargetMember.IsReadable
                ? mapperData.GetTargetMemberAccess().GetIsDefaultComparison()
                : null;
        }
    }
}