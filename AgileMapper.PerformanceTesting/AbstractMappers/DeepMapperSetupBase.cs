namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using static TestClasses.Deep;

    public abstract class DeepMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "deeps";

        protected override object Execute() => SetupDeepMapper((Customer)SourceObject);

        protected abstract CustomerDto SetupDeepMapper(Customer customer);
    }
}