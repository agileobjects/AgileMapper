namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Complex;

    public class AutoMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Foo, Foo>();
            });

            _mapper = config.CreateMapper();
        }

        protected override Foo Clone(Foo foo) => _mapper.Map<Foo, Foo>(foo);
    }
}