namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class Ef5QueryProviderSettings : LegacyEfQueryProviderSettings
    {
        public override bool SupportsToString => false;

        public override bool SupportsNonEntityNullConstants => false;

        protected override Type LoadCanonicalFunctionsType()
        {
            return GetTypeOrNull(
                "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Objects.EntityFunctions");
        }

        protected override Type LoadSqlFunctionsType()
        {
            return GetTypeOrNull(
                "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Objects.SqlClient.SqlFunctions");
        }

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
    }
}