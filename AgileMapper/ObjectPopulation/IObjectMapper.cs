namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IObjectMapper : IObjectMapperFunc
    {
        bool IsNullObject { get; }

        Expression MappingExpression { get; }

        LambdaExpression MappingLambda { get; }

        ObjectMapperData MapperData { get; }

        IEnumerable<IObjectMapper> SubMappers { get; }
    }
}