namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    internal abstract class UnflatteningMapperSetupBase : MapperSetupTestBase
    {
        protected override void Execute() => SetupUnflatteningMapper();

        protected abstract void SetupUnflatteningMapper();
    }
}