namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;

    internal interface IMappingExecutionContextInternal : IMappingExecutionContext
    {
        IObjectMappingData ToMappingData();
    }
}