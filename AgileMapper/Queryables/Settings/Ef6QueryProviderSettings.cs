namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq.Expressions;

    internal class Ef6QueryProviderSettings : DefaultQueryProviderSettings
    {
        public override bool SupportsToString => true;

#if !NET_STANDARD
        protected override Type LoadCanonicalFunctionsType()
            => GetTypeOrNull("EntityFramework", "System.Data.Entity.DbFunctions");

        protected override Type LoadSqlFunctionsType()
            => GetTypeOrNull("EntityFramework.SqlServer", "System.Data.Entity.SqlServer.SqlFunctions");

        public override Expression ConvertTryParseCall(MethodCallExpression call, Expression fallbackValue)
        {
            return this.TryGetDateTimeFromStringCall(call, fallbackValue, out var convertedCall)
                ? convertedCall
                : base.ConvertTryParseCall(call, fallbackValue);
        }
#endif
    }
}