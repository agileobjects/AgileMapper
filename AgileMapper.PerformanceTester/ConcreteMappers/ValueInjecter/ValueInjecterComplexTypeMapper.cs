using ViMapper = Omu.ValueInjecter.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using TestClasses;

    internal class ValueInjecterComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
        }

        protected override Foo Clone(Foo foo)
        {
            return ViMapper.Map<Foo>(foo);
        }
    }
}