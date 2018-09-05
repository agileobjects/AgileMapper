﻿namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Entities;

    internal class MapsterEntityMapperSetup : EntityMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void SetupEntityMapper()
        {
            TypeAdapterConfig<Warehouse, Warehouse>.NewConfig()
                // .Map(dest => dest.Foos, src => src.Foos ?? new List<Foo>())
                .Compile();

            new Warehouse().Adapt<Warehouse, Warehouse>();
        }

        protected override void Reset()
            => TypeAdapterConfig<Warehouse, Warehouse>.Clear();
    }
}
