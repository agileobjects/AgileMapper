namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNull
    {
        [Fact]
        public void ShouldApplyAUserConfiguredCondition()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .If((o, a) => string.IsNullOrWhiteSpace(a.Line1))
                    .MapToNull();

                var source = new CustomerViewModel { Name = "Bob" };
                var result = mapper.Map(source).ToANew<Customer>();

                result.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldRestrictAConfiguredMapToNullConditionBySourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Address>()
                    .If((o, a) => a.Line1 == "Null!")
                    .MapToNull();

                var nonMatchingSource = new Customer
                {
                    Name = "Jen",
                    Address = new Address { Line1 = "Null!" }
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Customer>();

                nonMatchingResult.Address.ShouldNotBeNull();
                nonMatchingResult.Address.Line1.ShouldBe("Null!");

                var matchingSource = new CustomerViewModel
                {
                    Name = "Frank",
                    AddressLine1 = "Null!"
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<Customer>();

                matchingResult.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldOverwriteAPropertyToNull()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<Address>()
                    .If(ctx => ctx.Target.Line1 == null)
                    .MapToNull();

                var nonMatchingSource = new CustomerViewModel
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "Places!"
                };
                var target = new Customer { Address = new Address { Line1 = "Home!" } };

                mapper.Map(nonMatchingSource).Over(target);

                target.Address.ShouldNotBeNull();
                target.Address.Line1.ShouldBe("Places!");

                var matchingSource = new CustomerViewModel { Id = Guid.NewGuid() };

                mapper.Map(matchingSource).Over(target);

                target.Address.ShouldBeNull();

                var nullAddressTarget = new Customer();

                mapper.Map(matchingSource).Over(nullAddressTarget);

                target.Address.ShouldBeNull();
            }
        }
    }
}
