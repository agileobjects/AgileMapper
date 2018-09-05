namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Flattening;

    internal class AgileMapperUnflatteningMapper : UnflatteningMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return _mapper.Map(dto).ToANew<ModelObject>();
        }
    }
}