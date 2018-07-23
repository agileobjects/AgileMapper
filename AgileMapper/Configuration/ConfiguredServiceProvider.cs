namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredServiceProvider
    {
        private static readonly ParameterExpression _serviceType = Expression.Parameter(typeof(Type), "serviceType");
        private static readonly ParameterExpression _serviceName = Expression.Parameter(typeof(string), "name");
        private static readonly string[] _serviceProviderMethodNames = { "GetService", "GetInstance", "Resolve" };

        private readonly Func<Type, object> _unnamedServiceFactory;
        private readonly Func<Type, string, object> _namedServiceFactory;

        public ConfiguredServiceProvider(Func<Type, object> serviceFactory)
        {
            _unnamedServiceFactory = serviceFactory;
        }

        public ConfiguredServiceProvider(Func<Type, string, object> serviceFactory)
        {
            _namedServiceFactory = serviceFactory;
        }

        public bool IsNamed => _namedServiceFactory != null;

        public TService GetService<TService>(string name)
        {
            var hasName = !string.IsNullOrEmpty(name);

            if ((_unnamedServiceFactory == null) || hasName)
            {
                return GetNamedService<TService>(hasName ? name : null);
            }

            return (TService)_unnamedServiceFactory.Invoke(typeof(TService));

        }

        private TService GetNamedService<TService>(string name)
        {
            if (_namedServiceFactory != null)
            {
                return (TService)_namedServiceFactory.Invoke(typeof(TService), name);
            }

            var serviceName = typeof(TService).GetFriendlyName();

            throw new MappingConfigurationException((name != null)
                ? $"Unable to resolve {serviceName} service with name '{name}'; no named service provider has been configured"
                : $"Unable to resolve {serviceName} service; no service provider has been configured");
        }

        public static IEnumerable<ConfiguredServiceProvider> CreateFromOrThrow(object serviceProviderInstance)
        {
            var providerType = serviceProviderInstance.GetType();
            var providerObject = Expression.Constant(serviceProviderInstance, providerType);

            var providers = providerType
                .GetPublicInstanceMethods()
                .Project(m => GetServiceProviderOrNull(m, providerObject))
                .WhereNotNull()
                .ToArray();

            ThrowIfNoProvidersFound(providers);

            return providers;
        }

        private static ConfiguredServiceProvider GetServiceProviderOrNull(MethodInfo method, Expression providerObject)
        {
            if (Array.IndexOf(_serviceProviderMethodNames, method.Name) == -1)
            {
                return null;
            }

            var parameters = method.GetParameters();

            if (parameters.None() || (parameters[0].ParameterType != typeof(Type)))
            {
                return null;
            }

            if (parameters.HasOne())
            {
                return UnnamedServiceProviderFor(method, providerObject);
            }

            if (parameters[1].ParameterType != typeof(string))
            {
                return null;
            }

            if (parameters.Length == 2)
            {
                return NamedServiceProviderFor(method, providerObject);
            }

            for (var i = 2; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (!(parameter.IsOptional || parameter.IsParamsArray()))
                {
                    return null;
                }
            }

            return NamedServiceProviderFor(method, providerObject, parameters);
        }

        private static ConfiguredServiceProvider UnnamedServiceProviderFor(MethodInfo method, Expression providerObject)
        {
            var getServiceCall = Expression.Call(providerObject, method, _serviceType);
            var getServiceLambda = Expression.Lambda<Func<Type, object>>(getServiceCall, _serviceType);

            return new ConfiguredServiceProvider(getServiceLambda.Compile());
        }

        private static ConfiguredServiceProvider NamedServiceProviderFor(MethodInfo method, Expression providerObject)
        {
            var getServiceCall = Expression.Call(providerObject, method, _serviceType, _serviceName);
            var getServiceLambda = Expression.Lambda<Func<Type, string, object>>(getServiceCall, _serviceType, _serviceName);

            return new ConfiguredServiceProvider(getServiceLambda.Compile());
        }

        private static ConfiguredServiceProvider NamedServiceProviderFor(
            MethodInfo method,
            Expression providerObject,
            IEnumerable<ParameterInfo> parameters)
        {
            var extraArguments = parameters.Skip(2).Project<ParameterInfo, Expression>(p => Expression.Default(p.ParameterType));
            var arguments = new Expression[] { _serviceType, _serviceName }.Concat(extraArguments);

            var getServiceCall = Expression.Call(providerObject, method, arguments);
            var getServiceLambda = Expression.Lambda<Func<Type, string, object>>(getServiceCall, _serviceType, _serviceName);

            return new ConfiguredServiceProvider(getServiceLambda.Compile());
        }

        private static void ThrowIfNoProvidersFound(ICollection<ConfiguredServiceProvider> providers)
        {
        }
    }
}