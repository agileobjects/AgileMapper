namespace AgileObjects.AgileMapper.Plans
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IMappingPlanFunction
    {
        Expression GetExpression();

        string GetDescription();
    }
}