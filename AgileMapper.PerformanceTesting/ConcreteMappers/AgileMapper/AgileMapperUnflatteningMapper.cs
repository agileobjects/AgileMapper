namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Flattening;

    public class AgileMapperUnflatteningMapper : UnflatteningMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override ModelObject Unflatten(ModelDto dto)
            => _mapper.Map(dto).ToANew<ModelObject>();
    }
}