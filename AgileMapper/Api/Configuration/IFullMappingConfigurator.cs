namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    public interface IFullMappingConfigurator<TSource, TTarget> : IConditionalMappingConfigurator<TSource, TTarget>
    {
        PreEventMappingConfigStartingPoint<TSource, TTarget> Before { get; }

        PostEventMappingConfigStartingPoint<TSource, TTarget> After { get; }

        void SwallowAllExceptions();

        void PassExceptionsTo(Action<IMappingExceptionData<TSource, TTarget>> callback);

        DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget> Map<TDerivedSource>()
            where TDerivedSource : TSource;
    }
}