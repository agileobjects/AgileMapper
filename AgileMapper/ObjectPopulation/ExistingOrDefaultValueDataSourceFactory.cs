namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context)
            => context.TargetMember.IsReadable
                ? new ExistingMemberValueDataSource(context)
                : DefaultValueDataSourceFactory.Instance.Create(context);
    }
}