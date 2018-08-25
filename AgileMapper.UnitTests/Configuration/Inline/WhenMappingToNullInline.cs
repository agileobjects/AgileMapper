namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNullInline
    {
        [Fact]
        public void ShouldApplyAUserConfiguredConditionInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new CustomerViewModel { Name = "Bob" })
                    .ToANew<Customer>(cfg => cfg
                        .WhenMapping
                        .To<Address>()
                        .If((o, a) => a.Line1.IsNullOrWhiteSpace())
                        .MapToNull());

                result.Name.ShouldBe("Bob");
                result.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldErrorIfConditionsAreConfiguredForTheSameTypeInline()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<Address>()
                        .If((o, a) => a.Line1 == null)
                        .MapToNull();

                    mapper
                        .Map(new CustomerViewModel { Name = "Bob" })
                        .ToANew<Customer>(cfg => cfg
                            .WhenMapping
                            .To<Address>()
                            .If((o, a) => a.Line1 == string.Empty)
                            .MapToNull());
                }
            });

            configEx.Message.ShouldContain("already has");
        }
    }
}
