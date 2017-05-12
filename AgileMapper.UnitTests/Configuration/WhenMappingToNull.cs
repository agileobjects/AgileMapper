namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
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
    }
}
