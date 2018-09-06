namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Entities;

    public class AgileMapperEntityMapperSetup : EntityMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override void SetupEntityMapper()
            => _mapper.GetPlanFor<Warehouse>().ToANew<Warehouse>();

        protected override void Reset() => _mapper.Dispose();
    }
}
