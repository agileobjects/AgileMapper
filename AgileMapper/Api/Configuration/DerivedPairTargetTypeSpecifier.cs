namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal DerivedPairTargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public void To<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypePair = new DerivedTypePair(
                _configInfo,
                typeof(TTarget),
                typeof(TDerivedSource),
                typeof(TDerivedTarget));

            _configInfo.MapperContext.UserConfigurations.Add(derivedTypePair);
        }
    }
}