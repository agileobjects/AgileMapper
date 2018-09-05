namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    internal abstract class DeepMapperSetupBase : MapperSetupTestBase
    {
        protected override void Execute() => SetupDeepMapper();

        protected abstract void SetupDeepMapper();
    }
}