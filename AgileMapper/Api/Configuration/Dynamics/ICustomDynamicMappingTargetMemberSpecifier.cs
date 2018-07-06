#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System;
    using System.Linq.Expressions;

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
#endif