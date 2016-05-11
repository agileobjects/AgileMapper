namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;
    using ObjectPopulation;

    public class CallbackSpecifier<TSource, TTarget> : CallbackSpecifierBase
    {
        internal CallbackSpecifier(CallbackPosition callbackPosition, MapperContext mapperContext)
            : this(callbackPosition, new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal CallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(callbackPosition, configInfo)
        {
        }

        public ConditionSpecifier<TSource, TTarget> Call(Action<ITypedMemberMappingContext<TSource, TTarget>> callback)
        {
            //var callbackLambda = CreateCallbackLambda(callback);
            //var callback = CreateCallbackFactory(callbackLambda, parameterReplacementsFactory);

            //ConfigInfo.MapperContext.UserConfigurations.Add(callback);

            return new ConditionSpecifier<TSource, TTarget>(null);
        }
    }
}