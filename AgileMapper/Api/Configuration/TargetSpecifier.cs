namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Dictionaries;
#if FEATURE_DYNAMIC
    using Dynamics;
#endif
    using Projection;

    /// <summary>
    /// Provides options for specifying the target type and mapping rule set to which the configuration should
    /// apply.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    public class TargetSpecifier<TSource> : IProjectionResultSelector<TSource>
    {
        private readonly MappingConfigInfo _configInfo;

        internal TargetSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured in all mapping rule sets 
        /// (create new, overwrite, etc), to the target type specified by the given <typeparamref name="TTarget"/> 
        /// argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> To<TTarget>()
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the result 
        /// type specified by the given <typeparamref name="TResult"/> argument when mapping to new objects.
        /// </summary>
        /// <typeparam name="TResult">The result type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TResult> ToANew<TResult>()
            => UsingRuleSet<TResult>(Constants.CreateNew);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the given <typeparamref name="TTarget"/> argument when performing OnTo (merge) 
        /// mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> OnTo<TTarget>()
            => UsingRuleSet<TTarget>(Constants.Merge);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the given <typeparamref name="TTarget"/> argument when performing Over (overwrite) 
        /// mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> Over<TTarget>()
            => UsingRuleSet<TTarget>(Constants.Overwrite);

        /// <summary>
        /// Configure how this mapper performs query projections from the source type being configured to the 
        /// result type specified by the given <typeparamref name="TResult"/> argument.
        /// </summary>
        /// <typeparam name="TResult">The result type to which the configuration will apply.</typeparam>
        /// <returns>An IFullProjectionConfigurator with which to complete the configuration.</returns>
        public IFullProjectionConfigurator<TSource, TResult> ProjectedTo<TResult>()
            => UsingRuleSet<TResult>(Constants.Project);

        private MappingConfigurator<TSource, TTarget> UsingRuleSet<TTarget>(string name)
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForRuleSet(name));

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured for all
        /// mapping rule sets (create new, overwrite, etc), to target Dictionaries.
        /// </summary>
        public ITargetDictionaryMappingConfigurator<TSource, object> ToDictionaries => ToDictionariesWithValueType<object>();

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured for all
        /// mapping rule sets (create new, overwrite, etc), to target Dictionary{string, <typeparamref name="TValue"/>}
        /// instances.
        /// </summary>
        /// <typeparam name="TValue">
        /// The type of values contained in the Dictionary to which the configuration will apply.
        /// </typeparam>
        /// <returns>An ITargetDictionaryConfigSettings with which to continue the configuration.</returns>
        public ITargetDictionaryMappingConfigurator<TSource, TValue> ToDictionariesWithValueType<TValue>()
            => new TargetDictionaryMappingConfigurator<TSource, TValue>(_configInfo.ForAllRuleSets());

#if FEATURE_DYNAMIC
        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured for all
        /// mapping rule sets (create new, overwrite, etc), to target ExpandoObjects.
        /// </summary>
        public ITargetDynamicMappingConfigurator<TSource> ToDynamics
            => new TargetDynamicMappingConfigurator<TSource>(_configInfo);
#endif

        #region SourceIgnores

        /// <summary>
        /// Ignore all source members with a value matching the <paramref name="valuesFilter"/>, when
        /// mapping from the source type being configured to all target types. Matching member values
        /// will not be used to populate target members in mappings for all mapping rule sets (create
        /// new, overwrite, etc).
        /// </summary>
        /// <param name="valuesFilter">
        /// The matching function with which to test source values to determine if they should be
        /// ignored.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from the source
        /// type being configured.
        /// </returns>
        public IMappingConfigContinuation<TSource, object> IgnoreSources(
            Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
        {
            return To<object>().IgnoreSources(valuesFilter);
        }

        /// <summary>
        /// Ignore the given <paramref name="sourceMembers"/> when mapping from the source type being
        /// configured to all target types. The given member(s) will not be used to populate target
        /// members in mappings for all mapping rule sets (create new, overwrite, etc).
        /// </summary>
        /// <param name="sourceMembers">The source member(s) which should be ignored.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from the source
        /// type being configured.
        /// </returns>
        public IMappingConfigContinuation<TSource, object> IgnoreSource(
            params Expression<Func<TSource, object>>[] sourceMembers)
        {
            return To<object>().IgnoreSource(sourceMembers);
        }

        /// <summary>
        /// Ignore all source members of the given <typeparamref name="TMember">Type</typeparamref>
        /// when mapping from the source type being configured to all target types. Source members of
        /// this type will not be used to populate target members in mappings for all mapping rule
        /// sets (create new, overwrite, etc).
        /// </summary>
        /// <typeparam name="TMember">The Type of source member to ignore.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from the source
        /// type being configured.
        /// </returns>
        public IMappingConfigContinuation<TSource, object> IgnoreSourceMembersOfType<TMember>()
            => To<object>().IgnoreSourceMembersOfType<TMember>();

        /// <summary>
        /// Ignore all source members matching the given <paramref name="memberFilter"/> when mapping
        /// from the source type being configured to all target types. Source members matching the
        /// filter will not be used to populate target members in mappings for all mapping rule sets
        /// (create new, overwrite, etc).
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select source members to ignore.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from the source
        /// type being configured.
        /// </returns>
        public IMappingConfigContinuation<TSource, object> IgnoreSourceMembersWhere(
            Expression<Func<SourceMemberSelector, bool>> memberFilter)
        {
            return To<object>().IgnoreSourceMembersWhere(memberFilter);
        }

        #endregion
    }
}