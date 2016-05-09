namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class SourceCallbackSpecifier<TSource, TTarget> : ObjectCallbackSpecifier<TSource>
    {
        internal SourceCallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(callbackPosition, configInfo, typeof(TTarget), Callbacks.Source)
        {
        }
    }
}