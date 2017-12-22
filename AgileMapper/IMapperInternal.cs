namespace AgileObjects.AgileMapper
{
    internal interface IMapperInternal : IMapper
    {
        MapperContext Context { get; }
    }
}