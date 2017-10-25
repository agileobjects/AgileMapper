namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreationInline
    {
        [Fact]
        public void ShouldUseAConfiguredTargetInstanceFactoryInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(new Address { Line1 = "Some House" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Some Street" })
                        .And
                        .Ignore(a => a.Line2));

                result1.Line1.ShouldBe("Some House");
                result1.Line2.ShouldBe("Some Street");

                var result2 = mapper
                    .Map(new Address { Line1 = "Some Other House" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Some Street" })
                        .And
                        .Ignore(a => a.Line2));

                result2.Line1.ShouldBe("Some Other House");
                result2.Line2.ShouldBe("Some Street");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldReplaceATargetInstanceFactory()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ToANew<Address>()
                    .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2!" })
                    .And
                    .Ignore(a => a.Line2);

                var result = mapper
                    .Map(new Address { Line1 = "Line 1!" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2?!" }));

                result.Line1.ShouldBe("Line 1!");
                result.Line2.ShouldBe("Line 2?!");
            }
        }

        [Fact]
        public void ShouldReplaceATargetInstanceFactoryForANestedObject()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<Address>()
                    .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2!" })
                    .And
                    .Ignore(a => a.Line2);

                var result = mapper
                    .Map(new CustomerViewModel { Name = "Me", AddressLine1 = "Line 1!" })
                    .Over(new Customer { Name = "You" }, cfg => cfg
                           .CreateInstancesOf<Address>().Using(ctx => new Address { Line2 = "Line 2?!" }));

                result.Name.ShouldBe("Me");
                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("Line 1!");
                result.Address.Line2.ShouldBe("Line 2?!");
            }
        }
    }
}
