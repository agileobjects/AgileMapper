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

        bool SupportsGetValueOrDefault { get; }

        bool SupportsComplexTypeToNullComparison { get; }

        bool SupportsNonEntityNullConstants { get; }

        bool SupportsEnumerableMaterialisation { get; }

        Expression GetDefaultValueFor(Expression value);

        Expression ConvertToStringCall(MethodCallExpression call);

        Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue);

        Expression ConvertEmptyArrayCreation(NewArrayExpression newEmptyArray);
    }
}