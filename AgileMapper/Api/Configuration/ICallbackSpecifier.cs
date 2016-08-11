namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    public interface ICallbackSpecifier<TSource, TTarget>
    {
        MappingConfigContinuation<TSource, TTarget> Call(Action<IMappingData<TSource, TTarget>> callback);

        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback);

        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, int?> callback);
    }
}