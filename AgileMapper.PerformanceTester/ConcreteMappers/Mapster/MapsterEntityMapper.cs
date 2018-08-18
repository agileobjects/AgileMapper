﻿namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Entities;

    internal class MapsterEntityMapper : EntityMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<Warehouse, Warehouse>.NewConfig()
               // .Map(dest => dest.Foos, src => src.Foos ?? new List<Foo>())
                .Compile();
        }

        protected override Warehouse Clone(Warehouse warehouse)
            => warehouse.Adapt<Warehouse, Warehouse>();
    }
}
