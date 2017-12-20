namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Linq.Expressions;

    internal interface IConditionallyChainable
    {
        Expression PreCondition { get; }

        Expression Condition { get; }

        Expression Value { get; }
    }
}