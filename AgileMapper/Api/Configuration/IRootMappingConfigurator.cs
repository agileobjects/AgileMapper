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
        /// Configure a factory with which to create instances of the <typeparamref name="TObject"/> Type,
        /// in mappings from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TObject">The Type of object the creation of which is to be configured.</typeparam>
        /// <returns>
        /// An IMappingFactorySpecifier with which to configure the factory for  the 
        /// <typeparamref name="TObject"/> Type.
        /// </returns>
        IMappingFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>();

        /// <summary>
        /// Use the given <paramref name="factory"/> expression to map instances of the target type 
        /// being configured. The factory expression is passed a context object containing the current
        /// mapping's source and target objects, as well as other contextual information.
        /// This method configures a complete mapping - no further mapping of the target object is
        /// performed. To configure creation of the target object only and have the mapper populate
        /// its members, use
        /// <see cref="CreateInstancesUsing(Expression{Func{IMappingData{TSource, TTarget}, TTarget}})"/>.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to
        /// the source and target type being configured.
        /// </returns>
        /// <seealso cref="CreateInstancesUsing(Expression{Func{IMappingData{TSource, TTarget}, TTarget}})"/>
        /// <seealso cref="CreateInstancesUsing{TFactory}(TFactory)"/>
        IMappingConfigContinuation<TSource, TTarget> MapInstancesUsing(
            Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory);

        /// <summary>
        /// Use the given <paramref name="factory"/> function to map instances of the target type
        /// being configured. The following factory function signatures are supported:
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
        /// This method configures a complete mapping - no further mapping of the target object is
        /// performed. To configure creation of the target object, and have the mapper populate its 
        /// members, use
        /// <see cref="CreateInstancesUsing{TFactory}(TFactory)"/>.
        /// </summary>
        /// <param name="factory">
        /// The factory function to use to map instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to
        /// the source and target type being configured.
        /// </returns>
        /// <seealso cref="CreateInstancesUsing(Expression{Func{IMappingData{TSource, TTarget}, TTarget}})"/>
        /// <seealso cref="CreateInstancesUsing{TFactory}(TFactory)"/>
        IMappingConfigContinuation<TSource, TTarget> MapInstancesUsing<TFactory>(TFactory factory)
            where TFactory : class;

        /// <summary>
        /// Configure a factory with which to map instances of the <typeparamref name="TObject"/>
        /// type, in mappings from and to the source and target types being configured.
        /// This method configures a complete mapping - no further mapping of the target object is
        /// performed. To configure creation of the target object only and have the mapper populate
        /// its members, use <see cref="CreateInstancesOf{TObject}"/>.
        /// </summary>
        /// <typeparam name="TObject">The Type of object the creation of which is to be configured.</typeparam>
        /// <returns>
        /// An IMappingFactorySpecifier with which to configure the factory for  the 
        /// <typeparamref name="TObject"/> Type.
        /// </returns>
        /// <seealso cref="CreateInstancesOf{TObject}"/>
        IMappingFactorySpecifier<TSource, TTarget, TObject> MapInstancesOf<TObject>();

        /// <summary>
        /// Ignore all source members with a value matching the <paramref name="valuesFilter"/>, when
        /// mapping from and to the source and target types being configured. Matching member values
        /// will not be used to populate target members.
        /// </summary>
        /// <param name="valuesFilter">
        /// The matching function with which to test source values to determine if they should be
        /// ignored.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreSources(
            Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter);

        /// <summary>
        /// Ignore the given <paramref name="sourceMembers"/> when mapping from and to the source and
        /// target types being configured. The given member(s) will not be used to populate target
        /// members.
        /// </summary>
        /// <param name="sourceMembers">The source member(s) which should be ignored.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> IgnoreSource(
            params Expression<Func<TSource, object>>[] sourceMembers);

        /// <summary>
        /// Ignore all source members of the given <typeparamref name="TMember">Type</typeparamref>
        /// when mapping from and to the source and target types being configured. Source members of
        /// this type will not be used to populate target members.
        /// </summary>
        /// <typeparam name="TMember">The Type of source member to ignore.</typeparam>
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
        /// Ignore the given <paramref name="targetMembers"/> when mapping from and to the source and
        /// target types being configured. The given member(s) will not be populated.
        /// </summary>
        /// <param name="targetMembers">The target member(s) which should be ignored.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the
        /// source and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers);

        /// <summary>
        /// Ignore all target members of the given <typeparamref name="TMember">Type</typeparamref> when mapping
        /// from and to the source and target types being configured.
        /// </summary>
        /// <typeparam name="TMember">The Type of target member to ignore.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target types being configured.
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

        /// <summary>
        /// Configure a custom data source for the given <paramref name="targetMember"/> when mapping from and to the
        /// source and target types being configured. The factory expression is passed a context object containing the
        /// current mapping's source and target objects.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        ICustomDataSourceMappingConfigContinuation<TSource, TTarget> Map<TSourceValue, TTargetValue>(
            Expression<Func<TSource, TSourceValue>> valueFactoryExpression,
            Expression<Func<TTarget, TTargetValue>> targetMember);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed a context object containing the current 
        /// mapping's source and target objects.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceMappingConfigContinuation with which to control the reverse configuration, or further
        /// configure mappings from and to the source and target type being configured.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed the current mapping's source and target 
        /// objects.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a custom data source for a particular target member when mapping from and to the source and 
        /// target types being configured. The factory expression is passed the current mapping's source and target 
        /// objects and the current enumerable index, if applicable.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression);

        /// <summary>
        /// Configure a Func object to be mapped to a target member of the same Func signature.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of value returned by the given Func.</typeparam>
        /// <param name="valueFunc">The Func object to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc);

        /// <summary>
        /// Configure a constant value for a particular target member when mapping from and to the source and 
        /// target types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <param name="value">The constant value to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom constant value should be applied.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value);

        /// <summary>
        /// Configure a constant value for the given <paramref name="targetMember"/> when mapping from and to the
        /// source and target types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="value">The constant value to map to the configured target member.</param>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Map<TSourceValue, TTargetValue>(
            TSourceValue value,
            Expression<Func<TTarget, TTargetValue>> targetMember);
    }
}