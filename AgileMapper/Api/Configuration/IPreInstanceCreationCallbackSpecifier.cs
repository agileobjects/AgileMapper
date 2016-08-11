namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    public interface IPreInstanceCreationCallbackSpecifier<out TSource, TTarget>
    {
        void Call(Action<IMappingData<TSource, TTarget>> callback);

        void Call(Action<TSource, TTarget> callback);

        void Call(Action<TSource, TTarget, int?> callback);
    }
}