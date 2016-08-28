namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringConstructorDataSources
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantByParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Guid>>()
                    .To<PublicCtor<string>>()
                    .Map("Hello there!")
                    .ToCtor<string>();

                var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicCtor<string>>();

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionByParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Guid>>()
                    .To<PublicCtor<string>>()
                    .Map(ctx => ctx.Source.Value.ToString().Substring(0, 10))
                    .ToCtor<string>();

                var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicCtor<string>>();

                result.Value.ShouldBe(source.Value.ToString().Substring(0, 10));
            }
        }

        [Fact]
        public void ShouldErrorIfMissingParameterTypeSpecified()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var configurationException = Should.Throw<MappingConfigurationException>(() =>
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(Guid.NewGuid())
                        .ToCtor<string>());

                configurationException.Message.ShouldContain("No constructor parameter");
            }
        }

        [Fact]
        public void ShouldErrorIfUnconvertibleConstantSpecified()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var configurationException = Should.Throw<MappingConfigurationException>(() =>
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(DateTime.Today)
                        .ToCtor<Guid>());

                configurationException.Message.ShouldContain("Unable to convert");
            }
        }
    }
}