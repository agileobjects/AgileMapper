namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class ConfiguredServiceProvider
    {
        private static readonly ParameterExpression _serviceType = Expression.Parameter(typeof(Type), "serviceType");
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

        public TService GetService<TService>(string name)
        {
            return string.IsNullOrEmpty(name)
                ? (TService)_unnamedServiceFactory.Invoke(typeof(TService))
                : (TService)_namedServiceFactory.Invoke(typeof(TService), name);
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

            if (parameters.Length != 2)
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
            return null;
        }

        private static ConfiguredServiceProvider NamedServiceProviderFor(
            MethodInfo method,
            Expression providerObject,
            ParameterInfo[] parameters)
        {
            return null;
        }

        private static void ThrowIfNoProvidersFound(ICollection<ConfiguredServiceProvider> providers)
        {
        }
    }
}