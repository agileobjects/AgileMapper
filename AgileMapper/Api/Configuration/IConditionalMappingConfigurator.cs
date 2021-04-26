namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;

    /// <summary>
    /// Provides options for configuring a condition which must evaluate to true for the configuration
    /// to apply to mappings from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IConditionalMappingConfigurator<TSource, TTarget> : IRootMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The
        /// <paramref name="condition"/> Expression is passed a context object containing the current
        /// mapping's source and target objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IConditionalRootMappingConfigurator with which to complete the configuration.</returns>
        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The
        /// <paramref name="condition"/> Expression is passed the current mapping's source and target
        /// objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IConditionalRootMappingConfigurator with which to complete the configuration.</returns>
        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The
        /// <paramref name="condition"/> Expression is passed the current mapping's source and target
        /// objects and the current element index, if applicable.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IConditionalRootMappingConfigurator with which to complete the configuration.</returns>
        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);
    }

    /// <summary>
    /// Provides options for configuring a filter which must evaluate to true for the configuration
    /// to apply to mappings from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFilteredMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Configure a <paramref name="memberFilter"/> which target members must match for the
        /// configuration to apply.
        /// </summary>
        /// <param name="memberFilter">
        /// The matching function with which to select target members to which to apply the configuration.
        /// </param>
        /// <returns>An IConditionalMappingConfigurator with which to complete the configuration.</returns>
        IConditionalMappingConfigurator<TSource, TTarget> IfTargetMembersMatch(
            Expression<Func<TargetMemberSelector, bool>> memberFilter);

        /// <summary>
        /// Ignore all source members of the given <typeparamref name="TMember">type</typeparamref>
        /// when mapping from and to the source and target types being configured. Source members of
        /// this type will not be used to populate target members.
        /// </summary>
        /// <typeparam name="TMember">The type of source member to ignore.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreSourceMembersOfType<TMember>();

        /// <summary>
        /// Ignore all source members matching the given <paramref name="memberFilter"/> when mapping
        /// from and to the source and target types being configured. Source members matching the filter
        /// will not be used to populate target members.
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select source members to ignore.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreSourceMembersWhere(
            Expression<Func<SourceMemberSelector, bool>> memberFilter);

        /// <summary>
        /// Ignore all target members of the given <typeparamref name="TMember">type</typeparamref>
        /// when mapping from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TMember">The type of target member to ignore.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to
        /// the source and target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersOfType<TMember>();

        /// <summary>
        /// Ignore all target members matching the given <paramref name="memberFilter"/> when mapping
        /// from and to the source and target types being configured.
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select target members to ignore.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter);
    }
}