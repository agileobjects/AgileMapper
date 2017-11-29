namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Converters;
    using Extensions;
    using NetStandardPolyfills;

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

        public virtual bool SupportsToString => true;

        public virtual bool SupportsGetValueOrDefault => true;

        public virtual bool SupportsEmptyEnumerableCreation => true;

        public virtual Expression ConvertToStringCall(MethodCallExpression call)
            => call.Object.GetConversionTo<string>();

        public Expression ConvertGetValueOrDefaultCall(MethodCallExpression call)
            => Expression.Convert(call.Object, call.Type);

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

        public virtual Expression ConvertEmptyArrayCreation(NewArrayExpression newEmptyArray) => newEmptyArray;

        protected virtual Expression GetParseStringToDateTimeOrNull(MethodCallExpression call, Expression fallbackValue)
            => null;

        private static Expression GetConvertStringToGuid(MethodCallExpression guidTryParseCall, Expression fallbackValue)
        {
            var parseMethod = typeof(Guid)
                .GetPublicStaticMethod("Parse", parameterCount: 1);

            var sourceValue = guidTryParseCall.Arguments.First();
            var guidConversion = Expression.Call(parseMethod, sourceValue);

            if (fallbackValue.NodeType == ExpressionType.Default)
            {
                fallbackValue = DefaultExpressionConverter.Convert(fallbackValue);
            }

            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(sourceValue, nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, guidConversion, fallbackValue);

            return convertedOrFallback;
        }

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