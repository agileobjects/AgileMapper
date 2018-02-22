namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperComplexTypeMapperSetup : ComplexTypeMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override void Reset()
        {
            _mapper.Dispose();

            _mapper.WhenMapping.DisableObjectTracking();
        }

        protected override void SetupComplexTypeMapper()
        {
            _mapper.GetPlanFor<Foo>().ToANew<Foo>();

            _mapper.Map(new Foo()).ToANew<Foo>();
        }
    }
}