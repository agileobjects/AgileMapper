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

        public ConfiguredServiceProvider(Func<Type, object> serviceFactory, ConstantExpression providerObject = null)
            : this(providerObject)
        {
            ThrowIfNull(serviceFactory);

            _unnamedServiceFactory = serviceFactory;
        }

        public ConfiguredServiceProvider(Func<Type, string, object> serviceFactory, ConstantExpression providerObject = null)
            : this(providerObject)
        {
            ThrowIfNull(serviceFactory);

            _namedServiceFactory = serviceFactory;
        }

        private ConfiguredServiceProvider(ConstantExpression providerObject)
        {
            if (providerObject != null)
            {
                ProviderObject = providerObject.Value;
            }
        }

        public bool IsNamed => _namedServiceFactory != null;

        public object ProviderObject { get; }

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
            => (TService)_namedServiceFactory.Invoke(typeof(TService), name);

        public static IEnumerable<ConfiguredServiceProvider> CreateFromOrThrow(object serviceProviderInstance)
        {
            ThrowIfNull(serviceProviderInstance);

            var providerType = serviceProviderInstance.GetType();
            var providerObject = Expression.Constant(serviceProviderInstance, providerType);

            var unnamedServiceProviderFound = false;
            var namedServiceProviderFound = false;

            var providers = providerType
                .GetPublicInstanceMethods()
                .Project(method => GetServiceProviderOrNull(
                    method,
                    providerObject,
                    ref unnamedServiceProviderFound,
                    ref namedServiceProviderFound))
                .WhereNotNull()
                .ToArray();

            ThrowIfNoProvidersFound(providers, providerObject);

            return providers;
        }

        private static void ThrowIfNull(object serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new MappingConfigurationException(
                    "No Service Provider supplied",
                    new ArgumentNullException(nameof(serviceProvider)));
            }
        }

        private static ConfiguredServiceProvider GetServiceProviderOrNull(
            MethodInfo method,
            ConstantExpression providerObject,
            ref bool unnamedServiceProviderFound,
            ref bool namedServiceProviderFound)
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
                return UnnamedServiceProviderFor(method, providerObject, ref unnamedServiceProviderFound);
            }

            if (parameters[1].ParameterType != typeof(string))
            {
                return null;
            }

            if (parameters.Length == 2)
            {
                return NamedServiceProviderFor(method, providerObject, ref namedServiceProviderFound);
            }

            for (var i = 2; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (!(parameter.IsOptional || parameter.IsParamsArray()))
                {
                    return null;
                }
            }

            return NamedServiceProviderFor(method, providerObject, parameters, ref namedServiceProviderFound);
        }

        private static ConfiguredServiceProvider UnnamedServiceProviderFor(
            MethodInfo method,
            ConstantExpression providerObject,
            ref bool unnamedServiceProviderFound)
        {
            if (unnamedServiceProviderFound)
            {
                return null;
            }

            unnamedServiceProviderFound = true;

            var getServiceCall = Expression.Call(providerObject, method, _serviceType);
            var getServiceLambda = Expression.Lambda<Func<Type, object>>(getServiceCall, _serviceType);

            return new ConfiguredServiceProvider(getServiceLambda.Compile(), providerObject);
        }

        private static ConfiguredServiceProvider NamedServiceProviderFor(
            MethodInfo method,
            ConstantExpression providerObject,
            ref bool namedServiceProviderFound)
        {
            if (namedServiceProviderFound)
            {
                return null;
            }

            namedServiceProviderFound = true;

            var getServiceCall = Expression.Call(providerObject, method, _serviceType, _serviceName);
            var getServiceLambda = Expression.Lambda<Func<Type, string, object>>(getServiceCall, _serviceType, _serviceName);

            return new ConfiguredServiceProvider(getServiceLambda.Compile(), providerObject);
        }

        private static ConfiguredServiceProvider NamedServiceProviderFor(
            MethodInfo method,
            ConstantExpression providerObject,
            IEnumerable<ParameterInfo> parameters,
            ref bool namedServiceProviderFound)
        {
            if (namedServiceProviderFound)
            {
                return null;
            }

            namedServiceProviderFound = true;

            var extraArguments = parameters.Skip(2).Project<ParameterInfo, Expression>(p => Expression.Default(p.ParameterType));
            var arguments = new Expression[] { _serviceType, _serviceName }.Concat(extraArguments);

            var getServiceCall = Expression.Call(providerObject, method, arguments);
            var getServiceLambda = Expression.Lambda<Func<Type, string, object>>(getServiceCall, _serviceType, _serviceName);

            return new ConfiguredServiceProvider(getServiceLambda.Compile(), providerObject);
        }

        private static void ThrowIfNoProvidersFound(ICollection<ConfiguredServiceProvider> providers, ConstantExpression providerObject)
        {
            if (providers.Any())
            {
                return;
            }

            var providerType = providerObject.Type.GetFriendlyName();

            throw new MappingConfigurationException(
                $@"
No supported service provider methods were found on provider object of type {providerType}.
The given object must expose one of the following public, instance methods:
  - GetService(Type type)
  - GetService(Type type, string name)
  - GetInstance(Type type)
  - GetInstance(Type type, string name)
  - Resolve(Type type)
  - Resolve(Type type, string name)
Overloads with a 'name' parameter can also take one or more optional or params array parameters.".TrimStart());
        }
    }
}