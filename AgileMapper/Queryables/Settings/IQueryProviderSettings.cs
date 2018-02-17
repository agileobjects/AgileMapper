﻿namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq.Expressions;

    internal interface IQueryProviderSettings
    {
        Type CanonicalFunctionsType { get; }

        Type SqlFunctionsType { get; }

        bool SupportsToString { get; }

        bool SupportsToStringWithFormat { get; }

        bool SupportsStringEqualsIgnoreCase { get; }

        bool SupportsStringToEnumConversion { get; }
        
        bool SupportsEnumToStringConversion { get; }

        bool SupportsGetValueOrDefault { get; }

        bool SupportsComplexTypeToNullComparison { get; }

        bool SupportsNonEntityNullConstants { get; }

        bool SupportsEnumerableMaterialisation { get; }

        Expression ConvertToStringCall(MethodCallExpression call);

        Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue);

        Expression ConvertStringEqualsIgnoreCase(MethodCallExpression call);

        Expression ConvertStringToEnumConversion(ConditionalExpression conversion);
    }
}