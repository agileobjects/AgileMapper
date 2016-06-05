namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;
    using ObjectPopulation;

    internal class CallbackSpecifier<TSource, TTarget> : CallbackSpecifierBase
    {
        public CallbackSpecifier(CallbackPosition callbackPosition, MapperContext mapperContext)
            : this(callbackPosition, new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        public CallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(callbackPosition, configInfo)
        {
        }

        public void Call(Action<ITypedMemberMappingContext<TSource, TTarget>> callback)
        {
            //var callbackLambda = CreateCallbackLambda(callback);
            //var callback = CreateCallbackFactory(callbackLambda, parameterReplacementsFactory);

            //ConfigInfo.MapperContext.UserConfigurations.Add(callback);
        }
    }
}