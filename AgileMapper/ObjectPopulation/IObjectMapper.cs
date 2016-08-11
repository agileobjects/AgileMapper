namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal interface IObjectMapper<out TTarget>
    {
        LambdaExpression MappingLambda { get; }

        TTarget Execute(IObjectMapperCreationData data);
    }
}