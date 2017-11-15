namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
#endif
    using System.Linq.Expressions;

    internal interface IQueryProviderSettings
    {
#if !NET_STANDARD
        Type CanonicalFunctionsType { get; }

        Type SqlFunctionsType { get; }
#endif
        bool SupportsStringEqualsIgnoreCase { get; }

        bool SupportsToString { get; }

        Expression ConvertToStringCall(MethodCallExpression call);

        Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue);
    }
}