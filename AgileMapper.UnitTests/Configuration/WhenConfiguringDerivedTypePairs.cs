namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypePairs
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
    }
}
