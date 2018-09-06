namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    public abstract class DeepMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "deeps";

        protected override void Execute() => SetupDeepMapper();

        protected abstract void SetupDeepMapper();
    }
}