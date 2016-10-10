namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
        }

        protected override Foo Clone(Foo foo)
        {
            return foo.Adapt<Foo>();
        }
    }
}