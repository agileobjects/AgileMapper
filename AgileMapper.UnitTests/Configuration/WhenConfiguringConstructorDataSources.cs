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
        public void ShouldApplyAConfiguredExpressionByParameterName()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicCtor<long>>()
                    .Map((s, t) => s.Value * 2)
                    .ToCtor("value");

                var source = new PublicProperty<int> { Value = 111 };
                var result = mapper.Map(source).ToANew<PublicCtor<long>>();

                result.Value.ShouldBe(222);
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
        public void ShouldErrorIfAmbiguousParameterTypeSpecified()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var configurationException = Should.Throw<MappingConfigurationException>(() =>
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicTwoParamCtor<DateTime, DateTime>>()
                        .Map(DateTime.Today)
                        .ToCtor<DateTime>());

                configurationException.Message.ShouldContain("Multiple constructor parameters");
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