namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal interface IObjectMapper<TSource, TTarget>
    {
        LambdaExpression MappingLambda { get; }

        TTarget Execute(MappingData<TSource, TTarget> data);
    }
}