namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    /// <summary>
    /// Provides options for configuring values to map to a specified target in mappings from and to
    /// the given source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IMapSourceConfigurator<TSource, TTarget>
    {
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
        /// objects and the current element index, if applicable.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom value being configured.</typeparam>
        /// <param name="valueFactoryExpression">The expression to map to the configured target member.</param>
        /// <returns>
        /// An ICustomDataSourceTargetMemberSpecifier with which to specify the target member to which the 
        /// custom value should be applied.
        /// </returns>
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression);
    }
}