namespace AgileObjects.AgileMapper.PerformanceTester.NetCore21.ConcreteMappers.Mapster
{
    using global::Mapster;
    using PerformanceTesting.AbstractMappers;
    using static PerformanceTesting.TestClasses.Entities;

    public class MapsterEntityMapperSetup : EntityMapperSetupBase
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
