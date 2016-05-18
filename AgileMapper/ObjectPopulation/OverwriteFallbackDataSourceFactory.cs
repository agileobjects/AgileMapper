namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Members;

    internal class OverwriteFallbackDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new OverwriteFallbackDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context)
            => new DefaultValueDataSource(context.SourceMember, context.TargetMember.Type);
    }
}