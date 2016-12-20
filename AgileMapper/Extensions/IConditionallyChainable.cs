namespace AgileObjects.AgileMapper.Extensions
{
    using System.Linq.Expressions;

    internal interface IConditionallyChainable
    {
        Expression PreCondition { get; }

        Expression Condition { get; }

        Expression Value { get; }
    }
}