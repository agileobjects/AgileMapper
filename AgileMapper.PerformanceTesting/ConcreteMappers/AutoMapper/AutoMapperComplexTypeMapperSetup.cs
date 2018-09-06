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

        protected override void SetupComplexTypeMapper()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Foo, Foo>());

            Mapper.Map<Foo, Foo>(new Foo());
        }

        protected override void Reset() => Mapper.Reset();
    }
}