namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using MapperKeys;

    internal interface IObjectMapperFactoryData : IMappingContext
    {
        ObjectMapperKeyBase GetMapperKey();
    }
}