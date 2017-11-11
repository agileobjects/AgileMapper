﻿namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;
#endif

    internal class Ef5QueryProviderSettings : DefaultQueryProviderSettings
    {
#if !NET_STANDARD
        protected override Type LoadCanonicalFunctionsType()
            => GetTypeOrNull("System.Data.Entity", "System.Data.Objects.EntityFunctions");

        protected override Type LoadSqlFunctionsType()
            => GetTypeOrNull("System.Data.Entity", "System.Data.Objects.SqlClient.SqlFunctions");

        public override Expression ConvertToStringCall(MethodCallExpression call)
        {
            var sqlFunctionsType = SqlFunctionsType;

            if (sqlFunctionsType == null)
            {
                return base.ConvertToStringCall(call);
            }

            var stringConvertCall = GetStringConvertCall(call.Object, sqlFunctionsType);
            var trimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var trimCall = Expression.Call(stringConvertCall, trimMethod);

            return trimCall;
        }

        private static Expression GetStringConvertCall(Expression subject, Type sqlFunctionsType)
        {
            var subjectType = subject.Type.GetNonNullableType();

            if (subjectType == typeof(decimal))
            {
                return Expression.Call(
                    sqlFunctionsType.GetPublicStaticMethod("StringConvert", typeof(decimal?)),
                    subject);
            }

            if (subjectType != typeof(double))
            {
                subject = Expression.Convert(subject, typeof(double?));
            }

            return Expression.Call(
                sqlFunctionsType.GetPublicStaticMethod("StringConvert", typeof(double?)),
                subject);
        }

        public override Expression ConvertTryParseCall(MethodCallExpression call)
        {
            return this.TryGetDateTimeFromStringCall(call, out var convertedCall)
                ? convertedCall
                : base.ConvertTryParseCall(call);
        }
#endif
    }
}