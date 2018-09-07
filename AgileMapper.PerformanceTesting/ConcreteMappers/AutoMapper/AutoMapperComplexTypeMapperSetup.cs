namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Complex;

    public class AutoMapperComplexTypeMapperSetup : ComplexTypeMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override Foo SetupComplexTypeMapper(Foo foo)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Foo, Foo>());

            return Mapper.Map<Foo, Foo>(foo);
        }

        protected override void Reset() => Mapper.Reset();
    }
}