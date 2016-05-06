namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using ObjectPopulation;

    internal interface INullNestedAccessStrategy
    {
        IMemberPopulation Process(IMemberPopulation population);
    }
}