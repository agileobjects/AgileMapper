namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Flattening;

    public class AgileMapperFlatteningMapper : FlatteningMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override ModelDto Flatten(ModelObject model)
            => _mapper.Map(model).ToANew<ModelDto>();
    }
}