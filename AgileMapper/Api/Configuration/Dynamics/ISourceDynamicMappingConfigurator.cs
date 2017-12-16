namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for configuring mappings from an ExpandoObject to a given <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDynamicMappingConfigurator<TTarget>
    {
        /// <summary>
        /// Configure a custom source member for a particular target member when mapping from an ExpandoObject 
        /// to the target type being configured.
        /// </summary>
        /// <param name="sourceMemberName">
        /// The name of the source member from which to retrieve the value to map to the configured target member.
        /// </param>
        /// <returns>
        /// An ICustomDynamicMappingTargetMemberSpecifier with which to specify the target member for which the 
        /// member with the given <paramref name="sourceMemberName"/> should be used.
        /// </returns>
        ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapMember(string sourceMemberName);
    }

    /// <summary>
    /// Provides options for specifying a target member to which an ExpandoObject configuration should apply.
    /// </summary>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ICustomDynamicMappingTargetMemberSpecifier<TTarget>
    {
        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An ISourceDynamicMappingConfigContinuation to enable further configuration of mappings from 
        /// Dynamics to the target type being configured.
        /// </returns>
        ISourceDynamicMappingConfigContinuation<TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember);
    }
}