namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using static TestClasses.Flattening;

    public abstract class UnflatteningMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "unflats";

        protected override object Execute() => SetupUnflatteningMapper((ModelDto)SourceObject);

        protected abstract ModelObject SetupUnflatteningMapper(ModelDto dto);
    }
}