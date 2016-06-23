namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IConditionalDataSourceFactory : IDataSourceFactory
    {
        bool IsFor(IMemberMappingContext context);
    }
}