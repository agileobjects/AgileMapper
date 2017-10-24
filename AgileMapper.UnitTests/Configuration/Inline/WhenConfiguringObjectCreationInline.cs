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
    }
}
