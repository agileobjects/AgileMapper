namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Flattening;

    public class AutoMapperFlatteningMapper : FlatteningMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>();
            });

            _mapper = config.CreateMapper();
        }

        protected override ModelDto Flatten(ModelObject model)
            => _mapper.Map<ModelObject, ModelDto>(model);
    }
}