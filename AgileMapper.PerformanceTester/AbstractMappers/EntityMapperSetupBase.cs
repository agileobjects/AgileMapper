namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    internal abstract class EntityMapperSetupBase : MapperSetupTestBase
    {
        protected override void Execute() => SetupEntityMapper();

        protected abstract void SetupEntityMapper();
    }
}
