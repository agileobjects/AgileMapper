namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for specifying a target member to which a configuration option should apply.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ICustomDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps to and from the source and target
        /// types being configured. This property can be used to set up a series of configurations
        /// to be applied in sequence.
        /// </summary>
        IConditionalMapSourceConfigurator<TSource, TTarget> Then { get; }

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An ICustomDataSourceMappingConfigContinuation with which to control the reverse configuration, or further
        /// configure mappings from and to the source and target type being configured.
        /// </returns>
        ICustomDataSourceMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember);

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetSetMethod"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The type of the target set method's argument.</typeparam>
        /// <param name="targetSetMethod">The target set method to which to apply the configuration.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod);

        /// <summary>
        /// Apply the configuration to the constructor parameter with the type specified by the type
        /// argument.
        /// </summary>
        /// <typeparam name="TTargetParam">The target constructor parameter's type.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>();

        /// <summary>
        /// Apply the configuration to the constructor parameter with the specified <paramref name="parameterName"/>.
        /// </summary>
        /// <param name="parameterName">The target constructor parameter's name.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName);

        /// <summary>
        /// Map the configured source value to the target object being configured, after any matching
        /// source member has been mapped. To map the configured source value <em>without</em> mapping
        /// any matching source member, use <strong>ToTargetInstead()</strong>.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToTarget();

        /// <summary>
        /// Map the configured source value to the target object being configured, <em>instead of</em>
        /// mapping any matching source member. If this mapping configuration has an If() clause which
        /// evaluates to false during mapping, no mapping is performed.<br /><br />
        /// To map any matching source member <em>as well as</em> the configured source value, use
        /// <strong>ToTarget()</strong>.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToTargetInstead();

        /// <summary>
        /// Apply the configured source value to the target object being configured, mapping to a
        /// <typeparamref name="TDerivedTarget"/> instance. This convenience method supports configuring
        /// a custom to-target mapping at the same time as specifying a derived target Type, if a
        /// derived Type pairing is being configured.
        /// </summary>
        /// <typeparam name="TDerivedTarget">
        /// The <typeparamref name="TTarget"/>-derived Type to which to map the source object.
        /// </typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToTarget<TDerivedTarget>()
            where TDerivedTarget : TTarget;
    }
}