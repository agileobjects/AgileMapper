namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    internal class EfCore2QueryProviderSettings : DefaultQueryProviderSettings
    {
        public override bool SupportsToStringWithFormat => true;

        public override bool SupportsStringEqualsIgnoreCase => true;

        // EF Core translates navigation property-to-null comparisons to compare 
        // on the navigation property id, and then falls over rewriting the 
        // comparison binary by trying to compare the complex type to its id value
        // This is due to be fixed in 2.1.
        public override bool SupportsComplexTypeToNullComparison => false;
    }
}