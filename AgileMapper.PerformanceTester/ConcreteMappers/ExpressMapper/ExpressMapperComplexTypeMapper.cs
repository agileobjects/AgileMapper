using ExMapper = ExpressMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class ExpressMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            ExMapper.Register<Foo, Foo>();
            ExMapper.Compile();
        }

        protected override Foo Clone(Foo foo)
        {
            return ExMapper.Map<Foo, Foo>(foo);
        }
    }
}