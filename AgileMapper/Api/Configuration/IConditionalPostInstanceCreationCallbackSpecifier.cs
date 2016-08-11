namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public interface IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
        : IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
    {
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<IObjectCreationMappingData<TSource, TTarget, TObject>, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, TObject, int?, bool>> condition);
    }
}