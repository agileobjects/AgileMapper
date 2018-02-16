namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
            _mapper.WhenMapping.DisableObjectTracking();
        }

        protected override Foo Clone(Foo foo)
        {
            return _mapper.DeepClone(foo);
        }
    }
}