namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Members;

    internal class NullDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new NullDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context) => NullDataSource.Default;
    }
}