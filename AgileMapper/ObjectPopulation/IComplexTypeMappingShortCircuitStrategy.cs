namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IComplexTypeMappingShortCircuitStrategy
    {
        IEnumerable<Expression> GetConditions(Expression sourceObject, IObjectMappingContext omc);
    }
}