namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal interface IValueProviderFactory
    {
        ValueProvider Create(IMemberMappingContext context);
    }
}