namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulationGuardFactory
    {
        Expression GetPopulationGuardOrNull(IMemberMapperData mapperData);
    }
}