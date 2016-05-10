namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    public class ObjectCallbackSpecifier<TSource, TTarget, TInstance>
    {
        private readonly CallbackPosition _callbackPosition;
        private readonly MappingConfigInfo _configInfo;
        private readonly Type _creationTargetType;
        private readonly Dictionary<int, Func<IMemberMappingContext, Expression[]>> _parameterReplacementsFactoriesByParameterCount;

        internal ObjectCallbackSpecifier(
            CallbackPosition callbackPosition,
            MapperContext mapperContext,
            params Func<IMemberMappingContext, Expression[]>[] parameterReplacementsFactories)
            : this(
                  callbackPosition,
                  new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes(),
                  typeof(TInstance),
                  parameterReplacementsFactories)
        {
        }

        internal ObjectCallbackSpecifier(
            CallbackPosition callbackPosition,
            MappingConfigInfo configInfo,
            Type creationTargetType,
            params Func<IMemberMappingContext, Expression[]>[] parameterReplacementsFactories)
        {
            _callbackPosition = callbackPosition;
            _configInfo = configInfo;
            _creationTargetType = creationTargetType;

            _parameterReplacementsFactoriesByParameterCount = parameterReplacementsFactories
                .Select((f, i) => new { Index = i, Factory = f })
                .ToDictionary(kvp => kvp.Index + 1, kvp => kvp.Factory);
        }

        public ConditionSpecifier<TSource, TInstance> Call(Action<TInstance> callback)
        {
            return AddCallback(callback);
        }

        protected ConditionSpecifier<TSource, TInstance> AddCallback<TAction>(TAction callback)
        {
            var callbackConstant = Expression.Constant(callback);

            var parameters = typeof(TAction)
                .GetGenericArguments()
                .Select(t => Parameters.Create(t))
                .ToArray();

            var callbackLambda = Expression.Lambda(
                typeof(TAction),
                Expression.Invoke(callbackConstant, parameters.Cast<Expression>()),
                parameters);

            var parameterReplacementsFactory = _parameterReplacementsFactoriesByParameterCount[parameters.Length];

            var creationCallback = new ObjectCreationCallbackFactory(
                _configInfo,
                typeof(TTarget),
                _creationTargetType,
                _callbackPosition,
                callbackLambda,
                parameterReplacementsFactory);

            _configInfo.MapperContext.UserConfigurations.Add(creationCallback);

            return new ConditionSpecifier<TSource, TInstance>(creationCallback);
        }
    }
}