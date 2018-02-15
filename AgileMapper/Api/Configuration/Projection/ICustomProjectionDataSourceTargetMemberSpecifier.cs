namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for specifying a result member to which a configuration option should apply.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface ICustomProjectionDataSourceTargetMemberSpecifier<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Apply the configuration to the given <paramref name="resultMember"/>.
        /// </summary>
        /// <typeparam name="TResultValue">The target member's type.</typeparam>
        /// <param name="resultMember">The result member to which to apply the configuration.</param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the 
        /// source and target type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> To<TResultValue>(
            Expression<Func<TResultElement, TResultValue>> resultMember);

        /// <summary>
        /// Apply the configuration to the constructor parameter with the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTargetParam">The result constructor parameter's type.</typeparam>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source 
        /// and result type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> ToCtor<TTargetParam>();

        /// <summary>
        /// Apply the configuration to the constructor parameter with the specified <paramref name="parameterName"/>.
        /// </summary>
        /// <param name="parameterName">The result constructor parameter's name.</param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source and 
        /// result type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> ToCtor(string parameterName);
    }
}