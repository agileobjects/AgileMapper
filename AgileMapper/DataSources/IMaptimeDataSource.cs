namespace AgileObjects.AgileMapper.DataSources
{
    internal interface IMaptimeDataSource : IDataSource
    {
        bool WrapInFinalDataSource { get; }
    }
}