namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using TestClasses;

    internal class AutoMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Foo, Foo>());
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Map<Foo, Foo>(foo);
        }
    }
}