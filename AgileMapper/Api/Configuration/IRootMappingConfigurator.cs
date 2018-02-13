namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;

    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IRootMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Use the given <paramref name="factory"/> expression to create instances of the target type being 
        /// configured. The factory expression is passed a context object containing the current mapping's source
        /// and target objects.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> CreateInstancesUsing(
            Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory);

        /// <summary>
        /// Use the given <paramref name="factory"/> function to create instances of the target type being 
        /// configured. The following factory function signatures are supported:
        /// <para>
        /// Func&lt;TTarget&gt; - parameterless.
        /// </para>
        /// <para>
        /// Func&lt;IMappingData&lt;TSource, TTarget&gt;, TTarget&gt; - taking a context object containing the 
        /// current mapping's source and target objects.
        /// </para>
        /// <para>
        /// Func&lt;TSource, TTarget, TTarget&gt; - taking the source and target objects.
        /// </para>
        /// <para>
        /// Func&lt;TSource, TTarget, int?, TTarget&gt; - taking the source and target objects and the current 
        /// enumerable index, if applicable.
        /// </para>
        /// </summary>
        /// <param name="factory">
        /// The factory function to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> CreateInstancesUsing<TFactory>(TFactory factory) 
            where TFactory : class;

        /// <summary>
        /// Configure a factory to use to create instances of the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TObject">The type of object the creation of which is to be configured.</typeparam>
        /// <returns>
        /// An IFactorySpecifier with which to configure the factory for the type specified by the type argument.
        /// </returns>
        IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() 
            where TObject : class;

        /// <summary>
        /// Ignore the given <paramref name="targetMembers"/> when mappingfrom and to the source and target types 
        /// being configured.
        /// </summary>
        /// <param name="targetMembers">The target member(s) which should be ignored.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers);

        /// <summary>
        /// Ignore all target member(s) of the given <typeparamref name="TMember">Type</typeparamref> when mapping
        /// from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TMember">The Type of target member to ignore.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersOfType<TMember>();

        /// <summary>
        /// Ignore all target member(s) matching the given <paramref name="memberFilter"/> when mapping
        /// from and to the source and target types being configured.
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select target members to ignore.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target types being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed a context object containing the current 
        /// mapping's source and target objects.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomMappingDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed the current mapping's source and target 
        /// objects.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomMappingDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed the current mapping's source and target 
        /// objects and the current enumerable index, if applicable.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomMappingDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a Func object to be mapped to a target member of the same Func signature.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of value returned by the given Func.</typeparam>
        /// <param name="valueFunc">The Func object to map to the configured target member.</param>
        /// <returns>
        /// An ICustomMappingDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc);

        /// <summary>
        /// Configure a constant value for a particular target member when mapping from and to the source and 
        /// target types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <param name="value">The constant value to map to the configured target member.</param>
        /// <returns>
        /// An ICustomMappingDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom constant value should be applied.
        /// </returns>
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value);
    }
}