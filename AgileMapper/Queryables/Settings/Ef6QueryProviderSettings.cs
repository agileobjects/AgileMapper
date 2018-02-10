namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Converters;

    internal class Ef6QueryProviderSettings : DefaultQueryProviderSettings
    {
        private readonly Assembly _entityFrameworkAssembly;

        public Ef6QueryProviderSettings(Assembly entityFrameworkAssembly)
        {
            _entityFrameworkAssembly = entityFrameworkAssembly;
        }

        public override bool SupportsGetValueOrDefault => false;

        public override bool SupportsEnumerableMaterialisation => false;

        protected override Type LoadCanonicalFunctionsType()
            => GetTypeOrNull(_entityFrameworkAssembly, "System.Data.Entity.DbFunctions");

        protected override Type LoadSqlFunctionsType()
        {
            return GetTypeOrNull(
                "EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Entity.SqlServer.SqlFunctions");
        }

        protected override Expression GetParseStringToDateTimeOrNull(MethodCallExpression call, Expression fallbackValue)
            => this.GetCreateDateTimeFromStringOrNull(call, fallbackValue);

        public override Expression GetDefaultValueFor(Expression value)
            => DefaultValueConstantExpressionFactory.CreateFor(value);
    }
}