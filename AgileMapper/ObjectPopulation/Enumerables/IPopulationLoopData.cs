namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal interface IPopulationLoopData
    {
        bool NeedsContinueTarget { get; set; }

        LabelTarget ContinueLoopTarget { get; }

        Expression LoopExitCheck { get; }

        Expression GetSourceElementValue();

        Expression GetElementMapping(IObjectMappingData enumerableMappingData);

        Expression Adapt(LoopExpression loop);
    }
}