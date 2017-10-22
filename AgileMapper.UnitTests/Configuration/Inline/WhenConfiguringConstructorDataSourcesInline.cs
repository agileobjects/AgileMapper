namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringConstructorDataSourcesInline
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantByParameterTypeInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new PublicProperty<Guid> { Value = Guid.NewGuid() })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map("Hello there!")
                        .ToCtor<string>());

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldExtendConstructorDataSourceConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoParamCtor<int, int>>()
                    .Map(ctx => ctx.Source.Value1 * 2)
                    .ToCtor("value1");

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 2, Value2 = 6 })
                    .ToANew<PublicTwoParamCtor<int, int>>(cfg => cfg
                        .Map(ctx => ctx.Source.Value2 / 2)
                        .ToCtor("value2"));

                result1.Value1.ShouldBe(4);
                result1.Value2.ShouldBe(3);

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 3, Value2 = 8 })
                    .ToANew<PublicTwoParamCtor<int, int>>(cfg => cfg
                        .Map(ctx => ctx.Source.Value2 / 2)
                        .ToCtor("value2"));

                result2.Value1.ShouldBe(6);
                result2.Value2.ShouldBe(4);
            }
        }
    }
}
