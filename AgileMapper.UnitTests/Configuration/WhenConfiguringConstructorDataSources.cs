namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringConstructorDataSources
    {
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
    }
}