namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public interface IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>
        : IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>
    {
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, TInstance, int?, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<ITypedObjectMappingContext<TSource, TTarget, TInstance>, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}