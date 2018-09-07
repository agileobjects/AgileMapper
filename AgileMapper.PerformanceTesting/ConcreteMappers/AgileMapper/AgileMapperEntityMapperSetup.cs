namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Entities;

    public class AgileMapperEntityMapperSetup : EntityMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override Warehouse SetupEntityMapper(Warehouse warehouse)
        {
            _mapper.GetPlanFor<Warehouse>().ToANew<Warehouse>();

            return _mapper.Map(warehouse).ToANew<Warehouse>();
        }

        protected override void Reset() => _mapper.Dispose();
    }
}
