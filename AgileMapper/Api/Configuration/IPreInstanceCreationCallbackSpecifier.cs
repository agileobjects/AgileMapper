namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public interface IPreInstanceCreationCallbackSpecifier<out TSource, out TTarget, out TObject> 
    {
        void Call(Action<ITypedObjectMappingContext<TSource, TTarget, TObject>> callback);

        void Call(Action<TSource, TTarget> callback);

        void Call(Action<TSource, TTarget, int?> callback);
    }
}