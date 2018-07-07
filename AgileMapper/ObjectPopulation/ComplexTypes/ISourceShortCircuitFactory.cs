namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ISourceShortCircuitFactory
    {
        bool IsFor(ObjectMapperData mapperData);

        Expression GetShortCircuit(IObjectMappingData mappingData);
    }
}