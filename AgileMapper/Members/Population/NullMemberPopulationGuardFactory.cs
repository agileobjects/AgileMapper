namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Members;

    internal class NullMemberPopulationGuardFactory : IMemberPopulationGuardFactory
    {
        public static readonly IMemberPopulationGuardFactory Instance = new NullMemberPopulationGuardFactory();

        public Expression GetPopulationGuardOrNull(IMemberMapperData mapperData) => null;
    }
}