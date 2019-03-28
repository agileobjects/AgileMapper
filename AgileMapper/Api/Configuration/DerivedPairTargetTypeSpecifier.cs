namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions.Extensions;

    internal class DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget> :
        IMappingDerivedPairTargetTypeSpecifier<TSource, TTarget>,
        IProjectionDerivedPairTargetTypeSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public DerivedPairTargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IMappingConfigContinuation<TSource, TTarget> To<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            return SetDerivedTargetType<TDerivedTarget>();
        }

        IProjectionConfigContinuation<TSource, TTarget> IProjectionDerivedPairTargetTypeSpecifier<TSource, TTarget>.To<TDerivedResult>()
            => SetDerivedTargetType<TDerivedResult>();

        private MappingConfigContinuation<TSource, TTarget> SetDerivedTargetType<TDerivedTarget>()
        {
            ThrowIfUnconstructable<TDerivedTarget>();

            var derivedTypePair = DerivedTypePair
                .For<TDerivedSource, TTarget, TDerivedTarget>(_configInfo);

            _configInfo.MapperContext.UserConfigurations.DerivedTypes.Add(derivedTypePair);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private void ThrowIfUnconstructable<TDerivedTarget>()
        {
            var mappingData = _configInfo.ToMappingData<TSource, TDerivedTarget>();

            var objectCreation = _configInfo
                .MapperContext
                .ConstructionFactory
                .GetNewObjectCreation(mappingData);

            if (objectCreation != null)
            {
                return;
            }

            if (!typeof(TDerivedTarget).IsAbstract())
            {
                ThrowUnableToCreate<TDerivedTarget>();
            }

            var configuredImplementationPairings = _configInfo
                .MapperContext
                .UserConfigurations
                .DerivedTypes.GetImplementationTypePairsFor(
                    _configInfo.ToMapperData(),
                    _configInfo.MapperContext);

            if (configuredImplementationPairings.None())
            {
                ThrowUnableToCreate<TDerivedTarget>();
            }
        }

        private static void ThrowUnableToCreate<TDerivedTarget>()
        {
            throw new MappingConfigurationException(
                $"Unable to create instances of Type '{typeof(TDerivedTarget).GetFriendlyName()}' - " +
                "configure a factory or derived Type pairing.");
        }
    }
}