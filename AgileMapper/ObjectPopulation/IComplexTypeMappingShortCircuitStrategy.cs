namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IComplexTypeMappingShortCircuitStrategy
    {
        bool SourceCanBeNull { get; }

        IEnumerable<Expression> GetConditions(IMemberMapperData mapperData);
    }
}