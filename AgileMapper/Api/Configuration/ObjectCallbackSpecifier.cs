namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    public class ObjectCallbackSpecifier<T>
    {
        private readonly CallbackPosition _callbackPosition;
        private readonly MappingConfigInfo _configInfo;
        private readonly Type _targetType;
        private readonly Dictionary<int, Func<IMemberMappingContext, Expression[]>> _parameterReplacementsFactoriesByParameterCount;

        internal ObjectCallbackSpecifier(
            CallbackPosition callbackPosition,
            MapperContext mapperContext,
            params Func<IMemberMappingContext, Expression[]>[] parameterReplacementsFactories)
            : this(
                  callbackPosition,
                  new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes(),
                  typeof(T),
                  parameterReplacementsFactories)
        {
        }

        internal ObjectCallbackSpecifier(
            CallbackPosition callbackPosition,
            MappingConfigInfo configInfo,
            Type targetType,
            params Func<IMemberMappingContext, Expression[]>[] parameterReplacementsFactories)
        {
            _callbackPosition = callbackPosition;
            _configInfo = configInfo;
            _targetType = targetType;

            _parameterReplacementsFactoriesByParameterCount = parameterReplacementsFactories
                .Select((f, i) => new { Index = i, Factory = f })
                .ToDictionary(kvp => kvp.Index + 1, kvp => kvp.Factory);
        }

        public void Call(Action<T> callback)
        {
            AddCallback(callback);
        }

        protected void AddCallback<TAction>(TAction callback)
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
                _targetType,
                _callbackPosition,
                callbackLambda,
                parameterReplacementsFactory);

            _configInfo.MapperContext.UserConfigurations.Add(creationCallback);
        }
    }
}