namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public class CallbackSpecifier<TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal CallbackSpecifier(MapperContext mapperContext)
            : this(new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal CallbackSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public void Call(Action<TTarget> callback)
        {
            var callbackConstant = Expression.Constant(callback);
            var createdInstanceParameter = Parameters.Create<TTarget>("createdInstance");
            var callbackInvocation = Expression.Invoke(callbackConstant, createdInstanceParameter);
            var callbackLambda = Expression.Lambda<Action<TTarget>>(callbackInvocation, createdInstanceParameter);

            var creationCallback = new ObjectCreationCallback(
                _configInfo,
                typeof(TTarget),
                callbackLambda);

            _configInfo.MapperContext.UserConfigurations.Add(creationCallback);
        }
    }
}