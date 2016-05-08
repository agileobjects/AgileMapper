namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    public class TargetCallbackSpecifier<TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly Type _targetType;

        internal TargetCallbackSpecifier(MapperContext mapperContext)
            : this(new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal TargetCallbackSpecifier(MappingConfigInfo configInfo)
            : this(configInfo, typeof(TTarget))
        {
        }

        internal TargetCallbackSpecifier(MappingConfigInfo configInfo, Type targetType)
        {
            _configInfo = configInfo;
            _targetType = targetType;
        }

        public void Call(Action<TTarget> callback)
        {
            AddCallback(
                Expression.Constant(callback),
                context => new Expression[] { context.TargetVariable },
                typeof(TTarget));
        }

        internal void AddCallback(
            Expression callbackConstant,
            Func<IMemberMappingContext, Expression[]> parameterReplacementsFactory,
            params Type[] parameterTypes)
        {
            var parameters = parameterTypes.Select(t => Parameters.Create(t)).ToArray();

            var callbackLambda = Expression.Lambda(
                callbackConstant.Type,
                Expression.Invoke(callbackConstant, parameters.Cast<Expression>()),
                parameters);

            var creationCallback = new ObjectCreationCallback(
                _configInfo,
                _targetType,
                callbackLambda,
                parameterReplacementsFactory);

            _configInfo.MapperContext.UserConfigurations.Add(creationCallback);
        }
    }
}