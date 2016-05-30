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

        // ReSharper disable PossibleNullReferenceException
        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetTypeInARootEnumerable()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<CustomerCtor>();

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerCtor>()
                    .CreateInstancesUsing(ctx => new CustomerCtor((decimal)ctx.Source.Discount * 3)
                    {
                        Number = ctx.EnumerableIndex + 1
                    });

                var source = new[] { new PersonViewModel(), new CustomerViewModel { Discount = 5 } };
                var result = mapper.Map(source).ToNew<Person[]>();

                var customer = result.Second() as CustomerCtor;
                customer.ShouldNotBeNull();
                customer.Discount.ShouldBe(15);
                customer.Number.ShouldBe(2);
            }
        }
        // ReSharper restore PossibleNullReferenceException

        private class CustomerCtor : Person
        {
            // ReSharper disable once UnusedMember.Local
            public CustomerCtor()
            {
            }

            public CustomerCtor(decimal discount)
            {
                Discount = discount;
            }

            public decimal Discount { get; }

            public int? Number { get; set; }
        }
    }
}
