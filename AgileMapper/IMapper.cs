namespace AgileObjects.AgileMapper
{
    using System;
    using Api;
    using Api.Configuration;

    public interface IMapper : IDisposable
    {
        PlanTargetTypeSelector<TSource> GetPlanFor<TSource>();

        PreEventConfigStartingPoint Before { get; }

        PostEventConfigStartingPoint After { get; }

        /// <summary>
        /// Configure how this mapper performs a mapping.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }

        TSource Clone<TSource>(TSource source) where TSource : class;

        dynamic Flatten<TSource>(TSource source) where TSource : class;

        TargetTypeSelector<TSource> Map<TSource>(TSource source);
    }
}