namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapperFunc
    {
        object Map(IObjectMappingData mappingData);
    }

    internal interface IObjectMapper : IObjectMapperFunc
    {
        Expression MappingExpression { get; }

        LambdaExpression MappingLambda { get; }

        ObjectMapperData MapperData { get; }
    }
}