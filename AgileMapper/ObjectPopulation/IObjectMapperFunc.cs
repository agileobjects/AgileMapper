namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapperFunc
    {
        LambdaExpression MappingLambda { get; }

        object Map(IObjectMappingData mappingData);
    }
}