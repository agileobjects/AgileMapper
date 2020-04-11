namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
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
                        Line2 = ctx.Source.Line2 + "!"
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
                    Line2 = srcAddr.Line2 + "?"
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
        public void ShouldUseAConfiguredFactoryForARootMappingConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address>()
                    .To<Address>()
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1,
                        Line2 = ctx.Source.Line1 + " again"
                    });

                var matchingSource = new Address { Line1 = "Over here" };
                var matchingResult = mapper.Map(matchingSource).ToANew<Address>();

                matchingResult.Line1.ShouldBe("Over here");
                matchingResult.Line2.ShouldBe("Over here again");

                var nonMatchingSource = new Address { Line1 = "Over here", Line2 = "Over there" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Address>();

                nonMatchingResult.Line1.ShouldBe("Over here");
                nonMatchingResult.Line2.ShouldBe("Over there");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredThreeParameterFactoryForANestedListElementMappingConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<MysteryCustomerViewModel, MysteryCustomer, int?, MysteryCustomer> mysteryCustomerMapper =
                    (srcMcVm, tgtMc, index) => new MysteryCustomer
                    {
                        Name = srcMcVm.Name,
                        Discount = index.GetValueOrDefault() * 2.0m,
                        Report = srcMcVm.Report
                    };

                mapper.WhenMapping
                    .From<MysteryCustomerViewModel>()
                    .To<MysteryCustomer>()
                    .If((mcVm, mc) => mcVm.Discount > 0.5)
                    .MapInstancesUsing(mysteryCustomerMapper);

                var source = new PublicField<ICollection<CustomerViewModel>>
                {
                    Value = new[]
                    {
                        new CustomerViewModel
                        {
                            Name = "Customer 1",
                            AddressLine1 = "Customer 1 House",
                            Discount = 0.5
                        },
                        new MysteryCustomerViewModel
                        {
                            Name = "Customer 2",
                            AddressLine1 = "Customer 2 House",
                            Discount = 0.75,
                            Report = "Pretty good!"
                        },
                        new MysteryCustomerViewModel
                        {
                            Name = "Customer 3",
                            AddressLine1 = "Customer 3 House",
                            Discount = 0.25,
                            Report = "Pretty great!"
                        }
                    }
                };

                var result = mapper.Map(source).ToANew<PublicProperty<IList<Customer>>>();

                result.Value.ShouldNotBeNull().Count.ShouldBe(3);

                var result1 = result.Value.First().ShouldBeOfType<Customer>();
                result1.Name.ShouldBe("Customer 1");
                result1.Address.ShouldNotBeNull().Line1.ShouldBe("Customer 1 House");
                result1.Discount.ShouldBe(0.5m);

                var result2 = result.Value.Second().ShouldBeOfType<MysteryCustomer>();
                result2.Name.ShouldBe("Customer 2");
                result2.Address.ShouldBeNull();
                result2.Discount.ShouldBe(2.0m);
                result2.Report.ShouldBe("Pretty good!");

                var result3 = result.Value.Third().ShouldBeOfType<MysteryCustomer>();
                result3.Name.ShouldBe("Customer 3");
                result3.Address.ShouldNotBeNull().Line1.ShouldBe("Customer 3 House");
                result3.Discount.ShouldBe(0.25m);
                result3.Report.ShouldBe("Pretty great!");
            }
        }

        [Fact]
        public void ShouldUseAConfiguedThreeParameterFactoryForARootArrayElementMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Address[], Address[], int?, Address> addressMapper = (srcAddrs, tgtAddrs, index) =>
                {
                    var sourceAddress = srcAddrs[index.GetValueOrDefault()];

                    return new Address
                    {
                        Line1 = (index + 1) + " " + sourceAddress.Line1,
                        Line2 = sourceAddress.Line2
                    };
                };

                mapper.WhenMapping
                    .From<Address[]>()
                    .ToANew<Address[]>()
                    .MapInstancesOf<Address>().Using(addressMapper);

                var source = new[]
                {
                    new Address { Line1 = "Line 1.1", Line2 = "Line 1.2" },
                    new Address { Line1 = "Line 2.1" }
                };

                var result = mapper.Map(source).ToANew<Address[]>();

                result.Length.ShouldBe(2);

                result.First().Line1.ShouldBe("1 Line 1.1");
                result.First().Line2.ShouldBe("Line 1.2");

                result.Second().Line1.ShouldBe("2 Line 2.1");
                result.Second().Line2.ShouldBeNull();
            }
        }
    }
}
