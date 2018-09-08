namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Ctor;

    public class AgileMapperCtorMapper : CtorMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override ConstructedObject Construct(ValueObject valueObject)
            => _mapper.Map(valueObject).ToANew<ConstructedObject>();
    }
}