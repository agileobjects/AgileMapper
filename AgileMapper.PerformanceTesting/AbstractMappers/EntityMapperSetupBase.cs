namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using static TestClasses.Entities;

    public abstract class EntityMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "ents";

        protected override object Execute() => SetupEntityMapper((Warehouse)SourceObject);

        protected abstract Warehouse SetupEntityMapper(Warehouse warehouse);
    }
}
