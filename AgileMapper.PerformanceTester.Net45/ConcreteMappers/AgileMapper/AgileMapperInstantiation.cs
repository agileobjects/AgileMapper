namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;

    internal class AgileMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => Mapper.CreateNew();
    }
}