namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Entities;

    public class AutoMapperEntityMapper : EntityMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.CreateMap<Associate, Associate>();
                //cfg.CreateMap<AssociateTag, AssociateTag>();
                //cfg.CreateMap<Branch, Branch>();
                //cfg.CreateMap<BranchTag, BranchTag>();
                //cfg.CreateMap<Location, Location>();
                //cfg.CreateMap<LocationTag, LocationTag>();
                //cfg.CreateMap<Movement, Movement>();
                //cfg.CreateMap<MovementTag, MovementTag>();
                //cfg.CreateMap<Product, Product>();
                //cfg.CreateMap<ProductTag, ProductTag>();
                //cfg.CreateMap<Tag, Tag>();
                //cfg.CreateMap<Warehouse, Warehouse>();
                //cfg.CreateMap<WarehouseTag, WarehouseTag>();
                //cfg.CreateMap<WarehouseProduct, WarehouseProduct>();
                //cfg.CreateMap<WarehouseProductTag, WarehouseProductTag>();
            });

            _mapper = config.CreateMapper();
        }

        protected override Warehouse Clone(Warehouse warehouse)
            => _mapper.Map<Warehouse, Warehouse>(warehouse);
    }
}
