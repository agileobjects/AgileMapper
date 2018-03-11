namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapper
    {
        bool IsNullObject { get; }

        LambdaExpression MappingLambda { get; }

        Expression MappingExpression { get; }

        ObjectMapperData MapperData { get; }

        object Map(IObjectMappingData mappingData);
    }
}