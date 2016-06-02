namespace AgileObjects.AgileMapper.DataSources
{
    internal interface IConfiguredDataSource : IDataSource
    {
        bool IsSameAs(IDataSource otherDataSource);
    }
}