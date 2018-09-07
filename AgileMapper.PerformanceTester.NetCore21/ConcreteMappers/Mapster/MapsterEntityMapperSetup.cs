namespace AgileObjects.AgileMapper.PerformanceTester.NetCore21.ConcreteMappers.Mapster
{
    // warehouse.Adapt<Warehouse, Warehouse>(); throws a StackOverflowException

    //using global::Mapster;
    //using PerformanceTesting.AbstractMappers;
    //using static PerformanceTesting.TestClasses.Entities;

    //public class MapsterEntityMapperSetup : EntityMapperSetupBase
    //{
    //    public override void Initialise()
    //    {
    //    }

    //    protected override Warehouse SetupEntityMapper(Warehouse warehouse)
    //    {
    //        TypeAdapterConfig<Warehouse, Warehouse>.NewConfig()
    //            // .Map(dest => dest.Foos, src => src.Foos ?? new List<Foo>())
    //            .Compile();

    //        return warehouse.Adapt<Warehouse, Warehouse>();
    //    }

    //    protected override void Reset()
    //        => TypeAdapterConfig<Warehouse, Warehouse>.Clear();
    //}
}
