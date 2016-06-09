namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapper<out TTarget>
    {
        LambdaExpression MappingLambda { get; }

        TTarget Execute(IObjectMappingContext objectMappingContext);
    }
}