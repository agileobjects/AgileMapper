namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class OverwriteFallbackValueProviderFactory : IValueProviderFactory
    {
        public static readonly IValueProviderFactory Instance = new OverwriteFallbackValueProviderFactory();

        public ValueProvider Create(IMemberMappingContext context) => ValueProvider.Default(context.TargetMember.Type);
    }
}