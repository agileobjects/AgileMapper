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
            _mapper.WhenMapping.DisableObjectTracking();
        }

        protected override void Reset()
        {
            _mapper.Dispose();
        }

        protected override void SetupComplexTypeMapper()
        {
            _mapper.GetPlanFor<Foo>().ToANew<Foo>();

            _mapper.Map(new Foo()).ToANew<Foo>();
        }
    }
}