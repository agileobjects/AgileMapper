namespace AgileObjects.AgileMapper.Extensions.Internal
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IConditionallyChainable
    {
        Expression Condition { get; }

        Expression Value { get; }
    }
}