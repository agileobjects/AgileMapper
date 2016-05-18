namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using ObjectPopulation;

    internal interface IPopulationProcessor
    {
        void Process(IMemberPopulation population);
    }
}