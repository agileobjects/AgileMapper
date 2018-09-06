namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    public abstract class EntityMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "ents";

        protected override void Execute() => SetupEntityMapper();

        protected abstract void SetupEntityMapper();
    }
}
