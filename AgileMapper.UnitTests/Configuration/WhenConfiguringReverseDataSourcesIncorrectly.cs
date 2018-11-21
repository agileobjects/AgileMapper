namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringReverseDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfRedundantOptOutSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id)
                        .To(pp => pp.Value)
                        .ButNotViceVersa();
                }
            });

            configEx.Message.ShouldContain("reverse");
            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfRedundantOptInSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ReverseConfiguredDataSources()
                        .AndWhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id)
                        .To(pp => pp.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("reversed");
            configEx.Message.ShouldContain("enabled by default");
        }
    }
}