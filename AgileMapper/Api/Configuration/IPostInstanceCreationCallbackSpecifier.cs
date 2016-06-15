namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public interface IPostInstanceCreationCallbackSpecifier<TSource, TTarget, out TObject> 
    {
        MappingConfigContinuation<TSource, TTarget> Call(
            Action<ITypedObjectCreationMappingContext<TSource, TTarget, TObject>> callback);

        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback);

        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, TObject> callback);

        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, TObject, int?> callback);
    }
}