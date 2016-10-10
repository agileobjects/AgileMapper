namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using TestClasses;

    internal class ValueInjecterComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Map<Foo>(foo);
        }
    }
}