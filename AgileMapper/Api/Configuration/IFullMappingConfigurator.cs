namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    public interface IFullMappingConfigurator<TSource, TTarget> : IConditionalMappingConfigurator<TSource, TTarget>
    {
        PreEventMappingConfigStartingPoint<TSource, TTarget> Before { get; }

        PostEventMappingConfigStartingPoint<TSource, TTarget> After { get; }

        void SwallowAllExceptions();

        void PassExceptionsTo(Action<ITypedMemberMappingExceptionContext<TSource, TTarget>> callback);

        DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget> Map<TDerivedSource>()
            where TDerivedSource : TSource;
    }
}