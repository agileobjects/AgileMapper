namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Entities;

    internal class AutoMapperEntityMapperSetup : EntityMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void SetupEntityMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Associate, Associate>();
                cfg.CreateMap<AssociateTag, AssociateTag>();
                cfg.CreateMap<Branch, Branch>();
                cfg.CreateMap<BranchTag, BranchTag>();
                cfg.CreateMap<Location, Location>();
                cfg.CreateMap<LocationTag, LocationTag>();
                cfg.CreateMap<Movement, Movement>();
                cfg.CreateMap<MovementTag, MovementTag>();
                cfg.CreateMap<Product, Product>();
                cfg.CreateMap<ProductTag, ProductTag>();
                cfg.CreateMap<Tag, Tag>();
                cfg.CreateMap<Warehouse, Warehouse>();
                cfg.CreateMap<WarehouseTag, WarehouseTag>();
                cfg.CreateMap<WarehouseProduct, WarehouseProduct>();
                cfg.CreateMap<WarehouseProductTag, WarehouseProductTag>();
            });

            Mapper.Map<Warehouse, Warehouse>(new Warehouse());
        }

        protected override void Reset() => Mapper.Reset();
    }
}
