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

        private MapperContext MapperContext => _configInfo.MapperContext;

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

            MapperContext.UserConfigurations.DerivedTypes.Add(derivedTypePair);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private void ThrowIfUnconstructable<TDerivedTarget>()
        {
            var mappingData = _configInfo.ToMappingData<TSource, TDerivedTarget>();

            if (mappingData.IsTargetConstructable() ||
                mappingData.IsConstructableFromToTargetDataSource())
            {
                return;
            }

            if (!typeof(TDerivedTarget).IsAbstract())
            {
                ThrowUnableToCreate<TDerivedTarget>();
            }

            var configuredImplementationPairings = MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetImplementationTypePairsFor(_configInfo.ToMemberContext());

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