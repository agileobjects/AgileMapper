namespace AgileObjects.AgileMapper.Queryables.Settings
{
    internal class EfCore2QueryProviderSettings : DefaultQueryProviderSettings
    {
        public override bool SupportsStringEqualsIgnoreCase => true;

        public override bool SupportsToString => true;
    }
}