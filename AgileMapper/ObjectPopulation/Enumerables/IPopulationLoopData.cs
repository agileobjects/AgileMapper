namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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