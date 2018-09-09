namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Complex;

    public class AgileMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
            _mapper.WhenMapping.DisableObjectTracking();
        }

        protected override Foo Clone(Foo foo) => _mapper.DeepClone(foo);
    }
}