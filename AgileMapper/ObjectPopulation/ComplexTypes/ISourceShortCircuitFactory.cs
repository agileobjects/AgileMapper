namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Linq.Expressions;

    internal interface ISourceShortCircuitFactory
    {
        bool IsFor(ObjectMapperData mapperData);

        Expression GetShortCircuit(IObjectMappingData mappingData);
    }
}