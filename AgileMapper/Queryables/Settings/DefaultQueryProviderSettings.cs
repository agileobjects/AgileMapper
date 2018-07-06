namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Converters;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DefaultQueryProviderSettings : IQueryProviderSettings
    {
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

        public virtual bool SupportsToString => true;

        public virtual bool SupportsToStringWithFormat => false;

        public virtual bool SupportsStringEqualsIgnoreCase => false;

        public virtual bool SupportsStringToEnumConversion => true;

        public virtual bool SupportsEnumToStringConversion => true;

        public virtual bool SupportsGetValueOrDefault => true;

        public virtual bool SupportsComplexTypeToNullComparison => true;

        public virtual bool SupportsNonEntityNullConstants => true;

        public virtual bool SupportsEnumerableMaterialisation => true;

        public virtual Expression ConvertToStringCall(MethodCallExpression call)
            => call.Object.GetConversionTo<string>();

        public Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue)
        {
            if (call.Method.DeclaringType == typeof(Guid))
            {
                return GetConvertStringToGuid(call, fallbackValue);
            }

            Expression conversion;

            if ((call.Method.DeclaringType == typeof(DateTime)) &&
                (conversion = GetParseStringToDateTimeOrNull(call, fallbackValue)) != null)
            {
                return conversion;
            }

            // ReSharper disable once PossibleNullReferenceException
            // Attempt to use Convert.ToInt32 - irretrievably unsupported in non-EDMX EF5 and EF6, 
            // but it at least gives a decent error message:
            var convertMethodName = "To" + call.Method.DeclaringType.Name;

            var convertMethod = typeof(Convert)
                .GetPublicStaticMethods(convertMethodName)
                .First(m => m.GetParameters().HasOne() && (m.GetParameters()[0].ParameterType == typeof(string)));

            conversion = Expression.Call(convertMethod, call.Arguments.First());

            return conversion;
        }

        public virtual Expression ConvertStringEqualsIgnoreCase(MethodCallExpression call) => call;

        #region ConvertTryParseCall Helpers

        private static Expression GetConvertStringToGuid(MethodCallExpression guidTryParseCall, Expression fallbackValue)
        {
            var parseMethod = typeof(Guid)
                .GetPublicStaticMethod("Parse", typeof(string));

            var sourceValue = guidTryParseCall.Arguments.First();
            var guidConversion = Expression.Call(parseMethod, sourceValue);
            fallbackValue = GetDefaultValueFor(fallbackValue);

            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(sourceValue, nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, guidConversion, fallbackValue);

            return convertedOrFallback;
        }

        protected static Expression GetDefaultValueFor(Expression value)
            => DefaultValueConstantExpressionFactory.CreateFor(value);

        protected virtual Expression GetParseStringToDateTimeOrNull(MethodCallExpression call, Expression fallbackValue)
            => null;

        #endregion

        public virtual Expression ConvertStringToEnumConversion(ConditionalExpression conversion) => conversion;

        protected static Type GetTypeOrNull(string loadedAssemblyName, string typeName)
            => GetTypeOrNull(GetAssemblyOrNull(loadedAssemblyName), typeName);

        private static Assembly GetAssemblyOrNull(string loadedAssemblyName)
        {
#if NET_STANDARD
            try
            {
                return Assembly.Load(new AssemblyName(loadedAssemblyName));
            }
            catch
            {
                return null;
            }
#else
            var assemblyName = loadedAssemblyName.Substring(0, loadedAssemblyName.IndexOf(','));

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
#endif
        }

        protected static Type GetTypeOrNull(Assembly assembly, string typeName) => assembly?.GetType(typeName);
    }
}