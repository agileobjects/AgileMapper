namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper.Register<Foo, Foo>();
            Mapper.Compile();
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Map<Foo, Foo>(foo);
        }
    }
}