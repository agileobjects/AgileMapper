using AgileObjects.AgileMapper.Configuration;

namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using AgileMapper.Configuration.Projection;

    internal class ProjectionConfigurator<TSourceElement, TTargetElement>
        : IFullProjectionInlineConfigurator<TSourceElement, TTargetElement>
    {
        private readonly MappingConfigInfo _configInfo;

        public ProjectionConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IFullProjectionInlineConfigurator<TSourceElement, TTargetElement> RecurseToDepth(int recursionDepth)
        {
            var depthSettings = new RecursionDepthSettings(_configInfo, recursionDepth);

            _configInfo.MapperContext.UserConfigurations.Add(depthSettings);
            return this;
        }
    }
}
