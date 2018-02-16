namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperFlatteningMapper : FlatteningMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return _mapper.Map(model).ToANew<ModelDto>();
        }
    }
}