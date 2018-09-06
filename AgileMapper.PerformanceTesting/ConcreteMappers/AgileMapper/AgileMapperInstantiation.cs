namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;

    public class AgileMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => Mapper.CreateNew();
    }
}