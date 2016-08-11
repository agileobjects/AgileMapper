namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IConditionalCallbackSpecifier<TSource, TTarget>
        : ICallbackSpecifier<TSource, TTarget>
    {
        ICallbackSpecifier<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition);

        ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition);

        ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}