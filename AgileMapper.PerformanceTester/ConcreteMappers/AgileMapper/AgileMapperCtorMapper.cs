namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperCtorMapper : CtorMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return _mapper.Map(valueObject).ToANew<ConstructedObject>();
        }
    }
}