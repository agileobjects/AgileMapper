namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    internal abstract class ComplexTypeMapperSetupBase : MapperSetupTestBase
    {
        protected override void Execute() => SetupComplexTypeMapper();

        protected abstract void SetupComplexTypeMapper();
    }
}
