namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
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
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map<Customer>()
                    .To<CustomerViewModel>();

                Person customer = new Customer();
                var viewModelResult = mapper.Map(customer).ToNew<PersonViewModel>();

                viewModelResult.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateAMemberDerivedTargetFromADerivedSourceMember()
        {
            using (var mapper = Mapper.Create())
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
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>();

                var source = new[] { new PersonViewModel(), new CustomerViewModel() };
                var result = mapper.Map(source).ToNew<ICollection<Person>>();

                result.First().ShouldBeOfType<Person>();
                result.Second().ShouldBeOfType<Customer>();
            }
        }

        [Fact]
        public void ShouldCreateADerivedTypeInAMemberEnumerableUsingRuntimeTypes()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
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
                var result = mapper.Map(source).ToNew<PublicSetMethod<IEnumerable<Person>>>();

                result.Value.First().ShouldBeOfType<Customer>();
                result.Value.Second().ShouldBeOfType<Person>();
            }
        }
    }
}
