namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Enables the selection of a derived target type to which to match a configured, derived source type.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object for which the derived type pair is being configured.
    /// </typeparam>
    /// <typeparam name="TDerivedSource">
    /// The type of derived source object for which the specified derived target type is being configured.
    /// </typeparam>
    /// <typeparam name="TTarget">
    /// The type of target object for which the derived type pair is being configured.
    /// </typeparam>
    public class DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal DerivedPairTargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Map the derived source type being configured to the derived target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TDerivedTarget">
        /// The derived target type to create for the configured derived source type.</typeparam>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        public MappingConfigContinuation<TSource, TTarget> To<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypePair = DerivedTypePair
                .For<TSource, TDerivedSource, TTarget, TDerivedTarget>(_configInfo);

            _configInfo.MapperContext.UserConfigurations.DerivedTypePairs.Add(derivedTypePair);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}