namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IObjectMapper
    {
        ParameterExpression MapperVariable { get; }

        LambdaExpression MapperLambda { get; }
    }

    internal interface IObjectMapper<out TTarget> : IObjectMapper
    {
        TTarget Execute(IObjectMapperCreationData data);
    }
}