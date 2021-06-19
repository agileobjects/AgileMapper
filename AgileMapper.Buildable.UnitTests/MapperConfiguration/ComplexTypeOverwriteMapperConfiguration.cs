namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class ComplexTypeOverwriteMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<Address>().Over<Address>();
        }
    }
}