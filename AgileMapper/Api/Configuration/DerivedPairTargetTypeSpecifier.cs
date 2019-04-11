namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
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

            if (IsConstructableUsing(mappingData))
            {
                return;
            }

            if (IsConstructableFromToTargetDataSources(mappingData, typeof(TDerivedTarget)))
            {
                return;
            }

            if (!typeof(TDerivedTarget).IsAbstract())
            {
                ThrowUnableToCreate<TDerivedTarget>();
            }

            var configuredImplementationPairings = MapperContext
                .UserConfigurations
                .DerivedTypes.GetImplementationTypePairsFor(
                    _configInfo.ToMapperData(),
                    MapperContext);

            if (configuredImplementationPairings.None())
            {
                ThrowUnableToCreate<TDerivedTarget>();
            }
        }

        private bool IsConstructableUsing(IObjectMappingData mappingData)
            => MapperContext.ConstructionFactory.GetNewObjectCreation(mappingData) != null;

        private bool IsConstructableFromToTargetDataSources(IObjectMappingData mappingData, Type derivedTargetType)
        {
            var toTargetDataSources = MapperContext
                .UserConfigurations
                .GetDataSourcesForToTarget(mappingData.MapperData);

            if (toTargetDataSources.None())
            {
                return false;
            }

            var constructionCheckMethod = typeof(DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>)
                .GetNonPublicInstanceMethod(nameof(IsConstructableFromDataSource));

            foreach (var dataSource in toTargetDataSources)
            {
                var isConstructable = (bool)constructionCheckMethod
                    .MakeGenericMethod(dataSource.SourceMember.Type, derivedTargetType)
                    .Invoke(this, Enumerable<object>.EmptyArray);

                if (isConstructable)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsConstructableFromDataSource<TDataSource, TDerivedTarget>()
        {
            var mappingData = _configInfo.ToMappingData<TDataSource, TDerivedTarget>();

            return IsConstructableUsing(mappingData);
        }

        private static void ThrowUnableToCreate<TDerivedTarget>()
        {
            throw new MappingConfigurationException(
                $"Unable to create instances of Type '{typeof(TDerivedTarget).GetFriendlyName()}' - " +
                "configure a factory or derived Type pairing.");
        }
    }
}