namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using ObjectPopulation;

    internal interface IPopulationProcessor
    {
        IEnumerable<IMemberPopulation> Process(IEnumerable<IMemberPopulation> populations);
    }
}