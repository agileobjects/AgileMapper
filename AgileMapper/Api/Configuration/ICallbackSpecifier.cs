namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    public interface ICallbackSpecifier<out TSource, out TTarget>
    {
        void Call(Action<ITypedMemberMappingContext<TSource, TTarget>> callback);

        void Call(Action<TSource, TTarget> callback);

        void Call(Action<TSource, TTarget, int?> callback);
    }
}