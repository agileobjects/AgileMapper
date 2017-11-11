namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;
#endif

    internal class Ef5QueryProviderSettings : DefaultQueryProviderSettings
    {
#if !NET_STANDARD
        public override Expression ConvertToString(MethodCallExpression toStringCall)
        {
            var sqlFunctionsType = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "System.Data.Entity")?
                .GetType("System.Data.Objects.SqlClient.SqlFunctions");

            if (sqlFunctionsType == null)
            {
                return base.ConvertToString(toStringCall);
            }

            var subject = toStringCall.Object;

            // ReSharper disable once PossibleNullReferenceException
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

            var subjectToString = Expression.Call(
                sqlFunctionsType.GetPublicStaticMethod("StringConvert", typeof(double?)),
                subject);

            return subjectToString;
        }
#endif
    }
}