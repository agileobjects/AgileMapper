namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByValueFilterIncorrectly
    {
        [Fact]
        public void ShouldErrorIfDuplicateSourceValueFilterConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreSources(c => c.If<int>(value => value == 999));

                    mapper.WhenMapping
                        .IgnoreSources(c => c.If<int>(value => value == 999));
                }
            });

            configEx.Message.ShouldContain("Source filter");
            configEx.Message.ShouldContain("If<int>(value => value == 999)");
            configEx.Message.ShouldContain("already been configured");
        }

        [Fact]
        public void ShouldErrorIfNoFiltersAreDefined()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.IgnoreSources(c => true);
                }
            });

            configEx.Message.ShouldContain("At least one source filter must be specified");
        }
    }
}
