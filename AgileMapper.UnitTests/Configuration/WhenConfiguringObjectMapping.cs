namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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
        public void ShouldUseAConfiguredFactoryForARootMapping()
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

                var source = new Address { Line1 = "Over here", Line2 = "Over there" };
                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("Over here!");
                result.Line2.ShouldBe("Over there!");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredTwoParameterFactoryForANestedMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                static Address MapAddress(Address srcAddr, Address tgtAddr) => new Address
                {
                    Line1 = srcAddr.Line1 + "?",
                    Line2 = srcAddr.Line2 + "?",
                };

                mapper.WhenMapping
                    .From<Address>()
                    .To<Address>()
                    .MapInstancesUsing((Func<Address, Address, Address>)MapAddress);

                var source = new PublicField<Address>
                {
                    Value = new Address { Line1 = "Here", Line2 = "There" }
                };
                var result = mapper.Map(source).ToANew<PublicField<Address>>();

                result.Value.ShouldNotBeNull();
                result.Value.Line1.ShouldBe("Here?");
                result.Value.Line2.ShouldBe("There?");
            }
        }

        [Fact]
        public void ShouldConditionallyUseAConfiguredFactoryForARootMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address>()
                    .To<Address>()
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1 + "!",
                        Line2 = ctx.Source.Line2 + "!",
                    });

                var source = new Address { Line1 = "Over here", Line2 = "Over there" };
                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("Over here!");
                result.Line2.ShouldBe("Over there!");
            }
        }
    }
}
