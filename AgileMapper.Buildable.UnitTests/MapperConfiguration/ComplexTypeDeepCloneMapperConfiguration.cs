namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class ComplexTypeDeepCloneMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<PublicTwoFields<Address, Address>>().ToANew<PublicTwoFields<Address, Address>>();
        }
    }
}