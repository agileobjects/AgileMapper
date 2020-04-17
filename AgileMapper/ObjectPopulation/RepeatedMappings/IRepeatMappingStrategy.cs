namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal interface IRepeatMappingStrategy
    {
        bool AppliesTo(IQualifiedMemberContext context);

        bool WillNotMap(IQualifiedMemberContext context);

        Expression GetMapRepeatedCallFor(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            ObjectMapperData declaredTypeMapperData);
    }
}
