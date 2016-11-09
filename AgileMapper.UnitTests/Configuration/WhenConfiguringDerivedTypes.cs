namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypes
    {
        [Fact]
        public void ShouldCreateARootDerivedTargetFromADerivedSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map<Customer>()
                    .To<CustomerViewModel>();

                Person customer = new Customer();
                var viewModelResult = mapper.Map(customer).ToANew<PersonViewModel>();

                viewModelResult.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateAMemberDerivedTargetFromADerivedSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map<Customer>()
                    .To<CustomerViewModel>();

                var source = new PublicProperty<Person> { Value = new Customer() };
                var target = new PublicSetMethod<PersonViewModel>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateAMemberGrandChildDerivedTargetFromADerivedSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>()
                    .And
                    .Map<MysteryCustomerViewModel>()
                    .To<MysteryCustomer>();

                var customerSource = new PublicProperty<PersonViewModel>
                {
                    Value = new CustomerViewModel { Discount = 0.5 }
                };

                var customerResult = mapper.Map(customerSource).ToANew<PublicField<Person>>();

                customerResult.Value.ShouldBeOfType<Customer>();
                ((Customer)customerResult.Value).Discount.ShouldBe(0.5);

                var mysteryCustomerSource = new PublicProperty<PersonViewModel>
                {
                    Value = new MysteryCustomerViewModel { Report = "Great!" }
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PublicField<Customer>>();

                mysteryCustomerResult.Value.ShouldBeOfType<MysteryCustomer>();
                ((MysteryCustomer)mysteryCustomerResult.Value).Report.ShouldBe("Great!");
            }
        }

        [Fact]
        public void ShouldCreateARootEnumerableDerivedTargetElementFromADerivedSourceElement()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>();

                var source = new[] { new PersonViewModel(), new CustomerViewModel() };
                var result = mapper.Map(source).ToANew<ICollection<Person>>();

                result.First().ShouldBeOfType<Person>();
                result.Second().ShouldBeOfType<Customer>();
            }
        }

        [Fact]
        public void ShouldCreateADerivedTypeInAMemberEnumerableUsingRuntimeTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .OnTo<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>();

                var source = new PublicProperty<object>
                {
                    Value = new Collection<object>
                    {
                        new CustomerViewModel { Name = "Fred" },
                        new PersonViewModel { Name = "Bob" }
                    }
                };
                var target = new PublicSetMethod<IEnumerable<Person>>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.First().ShouldBeOfType<Customer>();
                result.Value.Second().ShouldBeOfType<Person>();
            }
        }

        [Fact]
        public void ShouldCreateADerivedTypeInAnExistingMemberEnumerableUsingRuntimeTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .OnTo<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>();

                var personId = Guid.NewGuid();

                var source = new PublicGetMethod<object>(new List<PersonViewModel>
                {
                    new PersonViewModel { Id = personId, Name = "Bob" },
                    new CustomerViewModel { Name = "Fred" }
                });
                var target = new PublicField<object>
                {
                    Value = new Collection<Person>
                    {
                        new Person { Id = personId }
                    }
                };
                var result = mapper.Map(source).OnTo(target);

                var resultValues = (Collection<Person>)result.Value;
                resultValues.Count.ShouldBe(2);

                resultValues.First().ShouldBeOfType<Person>();
                resultValues.First().Id.ShouldBe(personId);
                resultValues.First().Name.ShouldBe("Bob");

                resultValues.Second().ShouldBeOfType<Customer>();
                resultValues.Second().Name.ShouldBe("Fred");
            }
        }

        [Fact]
        public void ShouldMapADerivedTypePairConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var exampleInstance = new { Name = default(string), Discount = default(decimal?), Report = default(string) };

                mapper
                    .WhenMapping
                    .From(exampleInstance)
                    .ToANew<PersonViewModel>()
                    .If(s => s.Source.Discount.HasValue)
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(x => !string.IsNullOrWhiteSpace(x.Source.Report))
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new
                {
                    Name = "???",
                    Discount = (decimal?).5m,
                    Report = "Lovely!"
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                mysteryCustomerResult.Name.ShouldBe("???");
                ((CustomerViewModel)mysteryCustomerResult).Discount.ShouldBe(0.5);
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Lovely!");

                var customerSource = new
                {
                    Name = "Firsty",
                    Discount = (decimal?)1,
                    Report = string.Empty
                };

                var customerResult = mapper.Map(customerSource).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                customerResult.Name.ShouldBe("Firsty");
                ((CustomerViewModel)customerResult).Discount.ShouldBe(1.0);

                var personSource = new
                {
                    Name = "Datey",
                    Discount = default(decimal?),
                    Report = default(string)
                };

                var personResult = mapper.Map(personSource).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Datey");
            }
        }

        [Fact]
        public void ShouldAccessAParentContextInAStandaloneMapper()
        {
            var source = new PublicProperty<object>
            {
                Value = new PersonViewModel { Name = "Fred" }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Person>>();

            result.Value.Name.ShouldBe("Fred");
        }
    }
}
