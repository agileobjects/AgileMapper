namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using ObjectPopulation;

    /// <summary>
    /// Provides options to configure the execution of a callback before a particular type of event for a 
    /// particular source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class PreEventMappingConfigStartingPoint<TSource, TTarget> : MappingConfigStartingPointBase<TSource, TTarget>
    {
        internal PreEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
            : base(configInfo, CallbackPosition.Before)
        {
        }

        /// <summary>
        /// Configure a callback to be executed before a mapping from and to the source and target types being 
        /// configured ends.
        /// </summary>
        public IConditionalCallbackSpecifier<TSource, TTarget> MappingBegins => CreateCallbackSpecifier();

        /// <summary>
        /// Configure a callback to be executed before the given <paramref name="targetMember"/> is populated
        /// during a mapping from and to the source and target types being configured.
        /// </summary>
        /// <param name="targetMember">
        /// The member of the target type being configured the population of which the callback execution should 
        /// precede.
        /// </param>
        public IConditionalCallbackSpecifier<TSource, TTarget> Mapping<TMember>(Expression<Func<TTarget, TMember>> targetMember)
            => CreateCallbackSpecifier(targetMember);

        /// <summary>
        /// Configure a callback to be executed before instances of any object are created during a mapping 
        /// from and to the source and target types being configured.
        /// </summary>
        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingInstances
            => CreateCallbackSpecifier<object>();

        /// <summary>
        /// Configure a callback to be executed before instances of the target type being configured are created 
        /// during a mapping from the source type being configured.
        /// </summary>
        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        /// <summary>
        /// Configure a callback to be executed before instances of the given type argument are created during a 
        /// mapping from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object the creation of which the callback execution should precede.
        /// </typeparam>
        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingInstancesOf<TObject>()
            where TObject : class
            => CreateCallbackSpecifier<TObject>();
    }
}