using AmMapper = AutoMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AutoMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            AmMapper.Initialize(cfg => cfg.CreateMap<Foo, Foo>());
        }

        protected override Foo Clone(Foo foo)
        {
            return AmMapper.Map<Foo, Foo>(foo);
        }
    }
}