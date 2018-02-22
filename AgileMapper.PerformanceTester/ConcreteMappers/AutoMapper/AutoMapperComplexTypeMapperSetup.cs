namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using TestClasses;

    internal class AutoMapperComplexTypeMapperSetup : ComplexTypeMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void Reset()
        {
            Mapper.Reset();
        }

        protected override void SetupComplexTypeMapper()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Foo, Foo>());

            Mapper.Map<Foo, Foo>(new Foo());
        }
    }
}