namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System;
#endif
    using System.Linq.Expressions;
    using Converters;

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
        public override Expression GetDefaultValueFor(Expression value)
            => DefaultValueConstantExpressionFactory.CreateFor(value);
    }
}