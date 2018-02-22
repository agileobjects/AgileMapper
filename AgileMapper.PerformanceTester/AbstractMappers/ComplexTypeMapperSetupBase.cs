namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;

    internal abstract class ComplexTypeMapperSetupBase : MapperSetupTestBase
    {
        protected override void Execute()
        {
            SetupComplexTypeMapper();
        }

        protected abstract void SetupComplexTypeMapper();
    }
}
