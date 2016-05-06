namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using ObjectPopulation;

    internal interface INullNestedAccessStrategy
    {
        IMemberPopulation ProcessSingle(IMemberPopulation singleMemberPopulation);

        IMemberPopulation ProcessMultiple(IMemberPopulation multipleMemberPopulation);
    }
}