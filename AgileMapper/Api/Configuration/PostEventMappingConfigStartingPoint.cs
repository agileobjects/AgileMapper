namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using ObjectPopulation;

    /// <summary>
    /// Provides options for configuring the execution of a callback after a particular type of event for a 
    /// particular source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class PostEventMappingConfigStartingPoint<TSource, TTarget> : MappingConfigStartingPointBase<TSource, TTarget>
    {
        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
            : base(configInfo, CallbackPosition.After)
        {
        }

        /// <summary>
        /// Configure a callback to be executed after a mapping from and to the source and target types being 
        /// configured ends.
        /// </summary>
        public IConditionalCallbackSpecifier<TSource, TTarget> MappingEnds => CreateCallbackSpecifier();

        /// <summary>
        /// Configure a callback to be executed after the given <paramref name="targetMember"/> is populated
        /// during a mapping from and to the source and target types being configured.
        /// </summary>
        /// <param name="targetMember">
        /// The member of the target type being configured the population of which the callback execution should 
        /// follow.
        /// </param>
        public IConditionalCallbackSpecifier<TSource, TTarget> Mapping<TMember>(Expression<Func<TTarget, TMember>> targetMember)
            => CreateCallbackSpecifier(targetMember);

        /// <summary>
        /// Configure a callback to be executed after instances of any object are created during a mapping 
        /// from and to the source and target types being configured.
        /// </summary>
        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        /// <summary>
        /// Configure a callback to be executed after instances of the target type being configured are created 
        /// during a mapping from the source type being configured.
        /// </summary>
        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        /// <summary>
        /// Configure a callback to be executed after instances of the given type argument are created during a 
        /// mapping from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object the creation of which the callback execution should follow.
        /// </typeparam>
        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> CreatingInstancesOf<TObject>()
            where TObject : class
            => CreateCallbackSpecifier<TObject>();
    }
}