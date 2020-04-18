namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IDataSourceSetInfo : IMappingContextOwner
    {
        IMemberMapperData MapperData { get; }
    }
}