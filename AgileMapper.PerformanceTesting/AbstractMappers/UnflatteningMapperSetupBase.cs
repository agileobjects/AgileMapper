namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    public abstract class UnflatteningMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "unflats";

        protected override void Execute() => SetupUnflatteningMapper();

        protected abstract void SetupUnflatteningMapper();
    }
}