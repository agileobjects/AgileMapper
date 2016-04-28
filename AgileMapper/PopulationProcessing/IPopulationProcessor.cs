namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using ObjectPopulation;

    internal interface IPopulationProcessor
    {
        IEnumerable<MemberPopulation> Process(IEnumerable<MemberPopulation> populations);
    }
}