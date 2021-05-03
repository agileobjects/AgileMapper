namespace AgileObjects.AgileMapper.DataSources
{
    internal interface IConfiguredDataSource : IDataSource
    {
        bool HasConfiguredMatcher { get; }
        
        bool HasConfiguredCondition { get; }

        bool IsSameAs(IDataSource otherDataSource);
    }
}