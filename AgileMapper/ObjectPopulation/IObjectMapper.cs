namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapper
    {
        LambdaExpression MappingLambda { get; }

        object Map(IObjectMappingData mappingData);

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
            int? enumerableIndex,
            IObjectMappingData parentMappingData);
    }
}