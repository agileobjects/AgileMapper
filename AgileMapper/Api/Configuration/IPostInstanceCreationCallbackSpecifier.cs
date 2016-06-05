namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public interface IPostInstanceCreationCallbackSpecifier<out TSource, out TTarget, out TInstance>
    {
        void Call(Action<ITypedObjectMappingContext<TSource, TTarget, TInstance>> callback);

        void Call(Action<TSource, TTarget> callback);

        void Call(Action<TSource, TTarget, TInstance> callback);

        void Call(Action<TSource, TTarget, TInstance, int?> callback);
    }
}