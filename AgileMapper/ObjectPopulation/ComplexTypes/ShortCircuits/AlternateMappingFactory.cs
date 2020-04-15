namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes.ShortCircuits
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal delegate Expression AlternateMappingFactory(
        IObjectMappingData mappingData,
        out bool isConditional);
}