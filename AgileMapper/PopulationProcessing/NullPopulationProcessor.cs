namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using ObjectPopulation;

    internal class NullPopulationProcessor : IPopulationProcessor
    {
        public static IPopulationProcessor Instance = new NullPopulationProcessor();

        public void Process(IMemberPopulation population)
        {
        }
    }
}