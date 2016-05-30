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

        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerCtor>()
                    .CreateInstancesUsing(ctx => new CustomerCtor((decimal)ctx.Source.Discount / 2));

                var nonMatchingSource = new PersonViewModel();
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<CustomerCtor>();

                nonMatchingResult.Discount.ShouldBeDefault();

                var matchingSource = new CustomerViewModel { Discount = 10 };
                var matchingResult = mapper.Map(matchingSource).ToNew<CustomerCtor>();

                matchingResult.Discount.ShouldBe(5);
            }
        }

        private class CustomerCtor : Person
        {
            public CustomerCtor()
            {
            }

            public CustomerCtor(decimal discount)
            {
                Discount = discount;
            }

            public decimal Discount { get; }
        }
    }
}
