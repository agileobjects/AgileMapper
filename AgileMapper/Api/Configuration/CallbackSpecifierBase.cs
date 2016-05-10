namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq;
    using System.Linq.Expressions;
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

        protected LambdaExpression CreateCallbackLambda<TAction>(TAction callback)
        {
            var callbackConstant = Expression.Constant(callback);

            var parameter = Parameters.Create(typeof(TAction).GetGenericArguments().First());

            var callbackLambda = Expression.Lambda(
                typeof(TAction),
                Expression.Invoke(callbackConstant, parameter),
                parameter);

            return callbackLambda;
        }
    }
}