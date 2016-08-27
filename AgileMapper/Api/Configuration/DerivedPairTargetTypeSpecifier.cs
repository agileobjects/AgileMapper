namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    public class DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal DerivedPairTargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

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