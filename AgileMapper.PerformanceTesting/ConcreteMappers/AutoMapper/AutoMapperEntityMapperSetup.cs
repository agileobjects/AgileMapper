﻿namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    // Mapper.Map<Warehouse, Warehouse>(warehouse); throws a StackOverflow exception

    //using AbstractMappers;
    //using global::AutoMapper;
    //using static TestClasses.Entities;

    //public class AutoMapperEntityMapperSetup : EntityMapperSetupBase
    //{
    //    public override void Initialise()
    //    {
    //    }

    //    protected override Warehouse SetupEntityMapper(Warehouse warehouse)
    //    {
    //        Mapper.Initialize(cfg =>
    //        {
    //            cfg.CreateMap<Associate, Associate>();
    //            cfg.CreateMap<AssociateTag, AssociateTag>();
    //            cfg.CreateMap<Branch, Branch>();
    //            cfg.CreateMap<BranchTag, BranchTag>();
    //            cfg.CreateMap<Location, Location>();
    //            cfg.CreateMap<LocationTag, LocationTag>();
    //            cfg.CreateMap<Movement, Movement>();
    //            cfg.CreateMap<MovementTag, MovementTag>();
    //            cfg.CreateMap<Product, Product>();
    //            cfg.CreateMap<ProductTag, ProductTag>();
    //            cfg.CreateMap<Tag, Tag>();
    //            cfg.CreateMap<Warehouse, Warehouse>();
    //            cfg.CreateMap<WarehouseTag, WarehouseTag>();
    //            cfg.CreateMap<WarehouseProduct, WarehouseProduct>();
    //            cfg.CreateMap<WarehouseProductTag, WarehouseProductTag>();
    //        });

    //        return Mapper.Map<Warehouse, Warehouse>(warehouse);
    //    }

    //    protected override void Reset() => Mapper.Reset();
    //}
}
