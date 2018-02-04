namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
    using System.Linq.Expressions;
#endif

    internal class Ef6QueryProviderSettings : DefaultQueryProviderSettings
    {
        public override bool SupportsGetValueOrDefault => false;

        public override bool SupportsEnumerableMaterialisation => false;

#if !NET_STANDARD
        protected override Type LoadCanonicalFunctionsType()
            => GetTypeOrNull("EntityFramework", "System.Data.Entity.DbFunctions");

        protected override Type LoadSqlFunctionsType()
            => GetTypeOrNull("EntityFramework.SqlServer", "System.Data.Entity.SqlServer.SqlFunctions");

        protected override Expression GetParseStringToDateTimeOrNull(MethodCallExpression call, Expression fallbackValue)
            => this.GetCreateDateTimeFromStringOrNull(call, fallbackValue);
#endif
    }
}