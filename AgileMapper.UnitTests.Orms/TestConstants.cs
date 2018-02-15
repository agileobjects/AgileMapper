namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;

    public static class TestConstants
    {
        public const string OrmCollectionName = "ORM Collection";

        public static string GetLocalDbConnectionString<TDbContext>()
        {
            var dbName = typeof(TDbContext).Name;

            if (dbName.EndsWith("Context", StringComparison.Ordinal))
            {
                dbName = dbName.Substring(0, dbName.Length - "Context".Length);
            }

            return "Data Source=(local);" +
                   "Initial Catalog=" + dbName + ";" +
                   "Integrated Security=True;" +
                   "MultipleActiveResultSets=True";
        }
    }
}