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
            var derivedTypePair = new DerivedTypePair(
                _configInfo.ForTargetType<TTarget>(),
                typeof(TDerivedSource),
                typeof(TDerivedTarget));

            _configInfo.MapperContext.UserConfigurations.DerivedTypePairs.Add(derivedTypePair);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}