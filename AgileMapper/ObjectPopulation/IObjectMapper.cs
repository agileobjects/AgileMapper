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

        object MapChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingData parentMappingData);

        object MapElement<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget targetElement,
            int enumerableIndex,
            IObjectMappingData parentMappingData);

        object MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingData parentMappingData);
    }
}