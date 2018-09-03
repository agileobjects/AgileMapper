namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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
    }
}