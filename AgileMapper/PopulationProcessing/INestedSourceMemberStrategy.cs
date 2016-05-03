namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using ObjectPopulation;

    internal interface INestedSourceMemberStrategy
    {
        IMemberPopulation Process(IMemberPopulation population);
    }
}