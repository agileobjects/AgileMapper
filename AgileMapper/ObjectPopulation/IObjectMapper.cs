namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapper
    {
        bool IsNullObject { get; }

        LambdaExpression MappingLambda { get; }

        Expression MappingExpression { get; }

        ObjectMapperData MapperData { get; }

        bool IsStaticallyCacheable(ObjectMapperKeyBase mapperKey);

        object Map(IObjectMappingData mappingData);

        void Reset();
    }
}