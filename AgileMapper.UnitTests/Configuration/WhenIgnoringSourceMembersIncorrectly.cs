namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersIncorrectly
    {
        [Fact]
        public void ShouldErrorIfRedundantSourceIgnoreIsSpecified()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .IgnoreSource(p => p.Name);

                    mapper.WhenMapping
                        .From<Customer>()
                        .To<CustomerViewModel>()
                        .IgnoreSource(c => c.Name);
                }
            });

            ignoreEx.Message.ShouldContain("has already been ignored");
        }

        [Fact]
        public void ShouldErrorIfNonPublicWriteOnlySimpleTypeMemberSpecified()
        {
            var configurationEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicWriteOnlyProperty<string>>()
                        .IgnoreSource(prop => prop.Value);
                }
            });

            configurationEx.Message.ShouldContain("not readable");
        }

        [Fact]
        public void ShouldErrorIfInvalidSourceMemberSpecified()
        {
            var configurationEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicReadOnlyProperty<string>>()
                        .IgnoreSource(prop => 2 * 2);
                }
            });

            configurationEx.Message.ShouldContain("Unable to determine source member");
        }
    }
}
