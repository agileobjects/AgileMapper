namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Entities;

    internal class AgileMapperEntityMapperSetup : EntityMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override void SetupEntityMapper()
            => _mapper.GetPlanFor<Warehouse>().ToANew<Warehouse>();

        protected override void Reset() => _mapper.Dispose();
    }
}
