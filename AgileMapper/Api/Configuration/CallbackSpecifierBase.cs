namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public abstract class CallbackSpecifierBase
    {
        internal CallbackSpecifierBase(CallbackPosition callbackPosition, MapperContext mapperContext)
            : this(callbackPosition, new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal CallbackSpecifierBase(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
        {
            CallbackPosition = callbackPosition;
            ConfigInfo = configInfo;
        }

        internal CallbackPosition CallbackPosition { get; }

        internal MappingConfigInfo ConfigInfo { get; }
    }
}