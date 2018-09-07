namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using static TestClasses.Complex;

    public abstract class ComplexTypeMapperSetupBase : MapperSetupTestBase
    {
        public override string Type => "compls";

        protected override object Execute() => SetupComplexTypeMapper((Foo)SourceObject);

        protected abstract Foo SetupComplexTypeMapper(Foo foo);
    }
}
