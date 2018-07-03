namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for specifying a target member to which a configuration option should apply.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
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
        /// Apply the configuration to the constructor parameter with the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTargetParam">The target constructor parameter's type.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>();

        /// <summary>
        /// Apply the configuration to the constructor parameter with the specified <paramref name="parameterName"/>.
        /// </summary>
        /// <param name="parameterName">The target constructor parameter's name.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName);

        /// <summary>
        /// Apply the configured source value to the target object being configured.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ToTarget();
    }
}