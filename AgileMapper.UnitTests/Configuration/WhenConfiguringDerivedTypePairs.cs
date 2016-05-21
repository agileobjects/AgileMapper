namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
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
    }
}
