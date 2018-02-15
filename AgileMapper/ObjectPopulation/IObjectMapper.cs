namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Recursion;

    internal interface IObjectMapper : IObjectMapperFunc
    {
        bool IsNullObject { get; }

        Expression MappingExpression { get; }

        ObjectMapperData MapperData { get; }

        IEnumerable<IRecursionMapperFunc> RecursionMapperFuncs { get; }
    }
}