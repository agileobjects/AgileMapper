namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Api.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenSettingInvalidConfiguration
    {
        [Fact]
        public void ShouldErrorIfUnconvertibleConstantSpecified()
        {
            Assert.Throws<MappingConfigurationException>(() =>
            {
                using (var mapper = new Mapper())
                {
                    mapper.When.Mapping
                        .From<PublicField<int>>()
                        .To<PublicField<DateTime>>()
                        .Map(new byte[] { 2, 4, 6, 8 })
                        .To(x => x.Value);
                }
            });
        }
    }
}
