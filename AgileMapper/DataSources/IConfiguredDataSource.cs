namespace AgileObjects.AgileMapper.DataSources
{
    internal interface IConfiguredDataSource : IDataSource
    {
        bool HasConfiguredCondition { get; }

        bool IsSameAs(IDataSource otherDataSource);
    }
}