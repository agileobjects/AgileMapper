namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDictionaryMappingIncorrectly
    {
        [Fact]
        public void ShouldErrorIfCustomMemberNameIsNull()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                Mapper.WhenMapping
                    .FromDictionaries()
                    .To<PublicField<string>>()
                    .MapMemberName(null)
                    .To(pf => pf.Value);
            });

            configEx.Message.ShouldContain("cannot be null");
        }
    }
}
