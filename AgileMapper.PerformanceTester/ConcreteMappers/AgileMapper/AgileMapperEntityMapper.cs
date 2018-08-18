namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Entities;

    internal class AgileMapperEntityMapper : EntityMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override Warehouse Clone(Warehouse warehouse)
            => _mapper.DeepClone(warehouse);
    }
}
