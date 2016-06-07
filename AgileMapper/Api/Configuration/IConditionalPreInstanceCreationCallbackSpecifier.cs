namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public interface IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
        : IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
    {
        IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<ITypedObjectMappingContext<TSource, TTarget, TObject>, bool>> condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}