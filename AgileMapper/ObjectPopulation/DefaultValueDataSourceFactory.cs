namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Members;

    internal class DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new DefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context)
            => new DefaultValueDataSource(context.SourceMember, context.TargetMember.Type);
    }
}