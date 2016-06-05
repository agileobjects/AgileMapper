namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    internal abstract class CallbackSpecifierBase
    {
        protected CallbackSpecifierBase(CallbackPosition callbackPosition, MapperContext mapperContext)
            : this(callbackPosition, new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        protected CallbackSpecifierBase(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
        {
            CallbackPosition = callbackPosition;
            ConfigInfo = configInfo;
        }

        protected CallbackPosition CallbackPosition { get; }

        protected MappingConfigInfo ConfigInfo { get; }
    }
}