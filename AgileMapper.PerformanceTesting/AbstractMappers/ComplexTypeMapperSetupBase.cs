namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    public abstract class ComplexTypeMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "compls";

        protected override void Execute() => SetupComplexTypeMapper();

        protected abstract void SetupComplexTypeMapper();
    }
}
