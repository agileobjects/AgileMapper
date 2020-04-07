namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringObjectMapping
    {
        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address>()
                    .To<Address>()
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1 + "!",
                        Line2 = ctx.Source.Line2 + "!",
                    });

                var matchingSource = new Address { Line1 = "Over here", Line2 = "Over there" };
                var matchingResult = mapper.Map(matchingSource).ToANew<Address>();

                matchingResult.Line1.ShouldBe("Over here!");
                matchingResult.Line2.ShouldBe("Over there!");
            }
        }
    }
}
