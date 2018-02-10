namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using Projection;

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
            var derivedTypePair = DerivedTypePair
                .For<TDerivedSource, TTarget, TDerivedTarget>(_configInfo);

            _configInfo.MapperContext.UserConfigurations.DerivedTypes.Add(derivedTypePair);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}