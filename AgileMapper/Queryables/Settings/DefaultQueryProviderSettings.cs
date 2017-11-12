namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
    using System.Linq;
#endif
    using System.Linq.Expressions;
#if !NET_STANDARD
    using System.Reflection;
#endif

    internal class DefaultQueryProviderSettings : IQueryProviderSettings
    {
#if !NET_STANDARD
        private readonly Lazy<Type> _canonicalFunctionsTypeLoader;
        private readonly Lazy<Type> _sqlFunctionsTypeLoader;

        public DefaultQueryProviderSettings()
        {
            _canonicalFunctionsTypeLoader = new Lazy<Type>(LoadCanonicalFunctionsType);
            _sqlFunctionsTypeLoader = new Lazy<Type>(LoadSqlFunctionsType);
        }

        protected virtual Type LoadCanonicalFunctionsType() => null;

        protected virtual Type LoadSqlFunctionsType() => null;

        public Type CanonicalFunctionsType => _canonicalFunctionsTypeLoader.Value;

        public Type SqlFunctionsType => _sqlFunctionsTypeLoader.Value;
#endif
        public virtual bool SupportsStringEqualsIgnoreCase => false;

        public virtual bool SupportsToString => false;

        public virtual Expression ConvertToStringCall(MethodCallExpression call) => call;

        public virtual Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue)
            => this.GetConvertToTypeCall(call);

#if !NET_STANDARD
        protected static Type GetTypeOrNull(string loadedAssemblyName, string typeName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == loadedAssemblyName)?
                .GetType(typeName);
        }
#endif
    }
}