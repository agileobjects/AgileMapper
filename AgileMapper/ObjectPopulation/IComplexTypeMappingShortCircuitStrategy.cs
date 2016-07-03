namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IComplexTypeMappingShortCircuitStrategy
    {
        bool SourceCanBeNull { get; }

        IEnumerable<Expression> GetConditions(IObjectMappingContext omc);
    }
}