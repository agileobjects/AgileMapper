namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal interface IPopulationLoopData
    {
        Expression LoopExitCheck { get; }

        Expression GetElementToAdd(IObjectMappingData enumerableMappingData);

        Expression Adapt(LoopExpression loop);
    }
}