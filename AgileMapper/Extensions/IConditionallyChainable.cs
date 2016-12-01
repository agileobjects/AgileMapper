namespace AgileObjects.AgileMapper.Extensions
{
    using System.Linq.Expressions;

    internal interface IConditionallyChainable
    {
        Expression Condition { get; }

        Expression Value { get; }
    }
}