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
            params KeyValuePair<int, Func<IMemberMappingContext, Expression[]>>[] parameterReplacementsFactoriesByParameterCount)
            : this(
                  callbackPosition,
                  new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes(),
                  typeof(T),
                  parameterReplacementsFactoriesByParameterCount)
        {
        }

        internal ObjectCallbackSpecifier(
            CallbackPosition callbackPosition,
            MappingConfigInfo configInfo,
            Type targetType,
            params KeyValuePair<int, Func<IMemberMappingContext, Expression[]>>[] parameterReplacementsFactoriesByParameterCount)
        {
            _callbackPosition = callbackPosition;
            _configInfo = configInfo;
            _targetType = targetType;

            _parameterReplacementsFactoriesByParameterCount =
                parameterReplacementsFactoriesByParameterCount.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void Call(Action<T> callback)
        {
            AddCallback(Expression.Constant(callback), typeof(T));
        }

        internal void AddCallback(Expression callbackConstant, params Type[] parameterTypes)
        {
            var parameters = parameterTypes.Select(t => Parameters.Create(t)).ToArray();

            var callbackLambda = Expression.Lambda(
                callbackConstant.Type,
                Expression.Invoke(callbackConstant, parameters.Cast<Expression>()),
                parameters);

            var parameterReplacementsFactory = _parameterReplacementsFactoriesByParameterCount[parameterTypes.Length];

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