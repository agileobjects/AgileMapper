namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Complex;

    public class AgileMapperComplexTypeMapperSetup : ComplexTypeMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override void SetupComplexTypeMapper()
            => _mapper.GetPlanFor<Foo>().ToANew<Foo>();

        protected override void Reset()
        {
            _mapper.Dispose();
            _mapper.WhenMapping.DisableObjectTracking();
        }
    }
}