namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
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

        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetTypeConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<PublicField<List<int>>>()
                    .If((cvm, c) => cvm.Discount > 0)
                    .CreateInstancesUsing(ctx => new PublicField<List<int>>
                    {
                        Value = new List<int> { (int)ctx.Source.Discount }
                    });

                var nonMatchingSource = new CustomerViewModel { Discount = 0 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicField<List<int>>>();

                nonMatchingResult.Value.ShouldBeNull();

                var matchingSource = new CustomerViewModel { Discount = 10 };
                var matchingResult = mapper.Map(matchingSource).ToNew<PublicField<List<int>>>();

                matchingResult.Value.ShouldNotBeNull();
                matchingResult.Value.ShouldHaveSingleItem();
                matchingResult.Value.First().ShouldBe(10);
            }
        }

        [Fact]
        public void ShouldUseConfiguredFactoriesForASpecifiedSourceAndTargetTypeConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<PublicField<ICollection<string>>>()
                    .CreateInstancesUsing(ctx => new PublicField<ICollection<string>>
                    {
                        Value = new Collection<string> { ctx.Source.Name }
                    });

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<PublicField<ICollection<string>>>()
                    .If((cvm, c) => cvm.Discount > 0)
                    .CreateInstancesUsing(ctx => new PublicField<ICollection<string>>
                    {
                        Value = new[] { ctx.Source.Name + $" ({ctx.Source.Discount})" }
                    });

                var unconditionalFactorySource = new CustomerViewModel { Name = "Steve", Discount = 0 };
                var unconditionalFactoryResult = mapper.Map(unconditionalFactorySource).ToNew<PublicField<ICollection<string>>>();

                unconditionalFactoryResult.Value.ShouldNotBeNull();
                unconditionalFactoryResult.Value.ShouldHaveSingleItem();
                unconditionalFactoryResult.Value.First().ShouldBe("Steve");

                var conditionalFactorySource = new CustomerViewModel { Name = "Alex", Discount = 1 };
                var conditionalFactoryResult = mapper.Map(conditionalFactorySource).ToNew<PublicField<ICollection<string>>>();

                conditionalFactoryResult.Value.ShouldNotBeNull();
                conditionalFactoryResult.Value.ShouldHaveSingleItem();
                conditionalFactoryResult.Value.First().ShouldBe("Alex (1)");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryWithAnObjectInitialiser()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicReadOnlyProperty<Address>>()
                    .CreateInstancesUsing(ctx => new PublicReadOnlyProperty<Address>(new Address())
                    {
                        Value = { Line1 = ctx.Source.Address.Line1, Line2 = ctx.Source.Address.Line2 }
                    });

                var source = new Person { Address = new Address { Line1 = "Here", Line2 = "There" } };
                var result = mapper.Map(source).ToNew<PublicReadOnlyProperty<Address>>();

                result.Value.Line1.ShouldBe("Here");
                result.Value.Line2.ShouldBe("There");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryWithAnListInitialiser()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicReadOnlyProperty<List<string>>>()
                    .CreateInstancesUsing(ctx => new PublicReadOnlyProperty<List<string>>(new List<string>())
                    {
                        Value = { ctx.Source.Id.ToString(), ctx.Source.Name }
                    });

                var source = new Person { Id = Guid.NewGuid(), Name = "Giles" };
                var result = mapper.Map(source).ToNew<PublicReadOnlyProperty<List<string>>>();

                result.Value.ShouldBe(source.Id.ToString(), "Giles");
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
