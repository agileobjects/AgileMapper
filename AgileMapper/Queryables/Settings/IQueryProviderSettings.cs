namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq.Expressions;

    internal interface IQueryProviderSettings
    {
        Type CanonicalFunctionsType { get; }

        Type SqlFunctionsType { get; }
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