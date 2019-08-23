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
    }
}
