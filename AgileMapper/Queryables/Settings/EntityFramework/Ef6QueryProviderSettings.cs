namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    using System;
    using System.Reflection;

    internal class Ef6QueryProviderSettings : LegacyEfQueryProviderSettings
    {
        private readonly Assembly _entityFrameworkAssembly;

        public Ef6QueryProviderSettings(Assembly entityFrameworkAssembly)
        {
            _entityFrameworkAssembly = entityFrameworkAssembly;
        }

        public override bool SupportsEnumToStringConversion => false;

        protected override Type LoadCanonicalFunctionsType()
            => GetTypeOrNull(_entityFrameworkAssembly, "System.Data.Entity.DbFunctions");

        protected override Type LoadSqlFunctionsType()
        {
            return GetTypeOrNull(
                "EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Entity.SqlServer.SqlFunctions");
        }
    }
}