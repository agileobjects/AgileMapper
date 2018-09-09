namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Complex;

    public class AgileMapperComplexTypeMapperSetup : ComplexTypeMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override Foo SetupComplexTypeMapper(Foo foo)
        {
            _mapper.GetPlanFor<Foo>().ToANew<Foo>();

            return _mapper.Map(foo).ToANew<Foo>();
        }

        protected override void Reset()
        {
            _mapper.Dispose();
            _mapper.WhenMapping.DisableObjectTracking();
        }
    }
}