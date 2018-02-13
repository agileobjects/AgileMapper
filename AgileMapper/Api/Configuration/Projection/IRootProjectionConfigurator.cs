namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for configuring projections from and to a given source and result type.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IRootProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Use the given <paramref name="factory"/> expression to create instances of the result type being 
        /// configured. The factory expression is passed the source element being projected, and must be 
        /// translatable by the QueryProvider being used.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the Type being configured.
        /// </param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of projections from and to the 
        /// source and result Type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> CreateInstancesUsing(
            Expression<Func<TSourceElement, TResultElement>> factory);

        /// <summary>
        /// Configure a factory to use to create instances of the <typeparamref name="TObject"/> Type.
        /// </summary>
        /// <typeparam name="TObject">The Type of object the creation of which is to be configured.</typeparam>
        /// <returns>
        /// An IProjectionFactorySpecifier with which to configure the factory for the 
        /// <typeparamref name="TObject"/> Type.
        /// </returns>
        IProjectionFactorySpecifier<TSourceElement, TResultElement, TObject> CreateInstancesOf<TObject>();

        /// <summary>
        /// Ignore the specified <paramref name="resultMembers"/> when projecting from and to the source and 
        /// result types being configured.
        /// </summary>
        /// <param name="resultMembers">The result member(s) which should be ignored.</param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source 
        /// and result type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> Ignore(
            params Expression<Func<TResultElement, object>>[] resultMembers);

        /// <summary>
        /// Ignore all result member(s) of the given <typeparamref name="TMember">Type</typeparamref> when projecting
        /// from and to the source and result types being configured.
        /// </summary>
        /// <typeparam name="TMember">The Type of result member to ignore.</typeparam>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of projections from and to the source and 
        /// result types being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> IgnoreTargetMembersOfType<TMember>();

        /// <summary>
        /// Ignore all result member(s) matching the given <paramref name="memberFilter"/> when projecting
        /// from and to the source and result types being configured.
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select result members to ignore.</param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source and 
        /// result types being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter);

        /// <summary>
        /// Configure a custom data source for a particular result member when mapping from and to the source and 
        /// result types being configured. The factory expression is passed the source element being projected.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured result member.</param>
        /// <returns>
        /// A CustomDataSourceTargetMemberSpecifier with which to specify the result member to which the custom 
        /// value should be applied.
        /// </returns>
        ICustomProjectionDataSourceTargetMemberSpecifier<TSourceElement, TResultElement> Map<TSourceValue>(
            Expression<Func<TSourceElement, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a constant value for a particular result member when projecting from and to the source and 
        /// result types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <param name="value">The constant value to map to the configured result member.</param>
        /// <returns>
        /// A CustomDataSourceTargetMemberSpecifier with which to specify the result member to which the custom 
        /// constant value should be applied.
        /// </returns>
        ICustomProjectionDataSourceTargetMemberSpecifier<TSourceElement, TResultElement> Map<TSourceValue>(
            TSourceValue value);
    }
}