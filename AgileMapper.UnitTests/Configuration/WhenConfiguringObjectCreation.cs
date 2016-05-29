namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreation
    {
        [Fact]
        public void ShouldUseAConfiguredFactoryForAGivenType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .InstancesOf<Address>()
                    .CreateUsing(() => new Address { Line2 = "Some Street" });

                var source = new PersonViewModel { AddressLine1 = "Some House" };
                var target = new Person();
                var result = mapper.Map(source).OnTo(target);

                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("Some House");
                result.Address.Line2.ShouldBe("Some Street");
            }
        }
    }
}
