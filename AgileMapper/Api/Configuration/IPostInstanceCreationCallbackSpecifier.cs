namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public interface IPostInstanceCreationCallbackSpecifier<out TSource, out TTarget, out TObject>
    {
        void Call(Action<ITypedObjectCreationMappingContext<TSource, TTarget, TObject>> callback);

        void Call(Action<TSource, TTarget> callback);

        void Call(Action<TSource, TTarget, TObject> callback);

        void Call(Action<TSource, TTarget, TObject, int?> callback);
    }
}