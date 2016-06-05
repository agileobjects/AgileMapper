namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>
        : IPreInstanceCreationCallbackSpecifier<TSource, TTarget>
    {
        IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, bool>> condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}