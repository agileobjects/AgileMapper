namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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

        [Fact]
        public void ShouldReplaceAConfiguredConstructorDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<long>>()
                    .To<PublicCtor<string>>()
                    .Map((s, t) => s.Value * 2)
                    .ToCtor<string>();

                var moreThanTwoResult = mapper
                    .Map(new PublicProperty<long> { Value = 3 })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map((s, t) => s.Value > 2 ? 2 : 1)
                        .ToCtor<string>());

                moreThanTwoResult.Value.ShouldBe("2");

                var lessThanTwoResult = mapper
                    .Map(new PublicProperty<long> { Value = 0 })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map((s, t) => s.Value > 2 ? 2 : 1)
                        .ToCtor<string>());

                lessThanTwoResult.Value.ShouldBe("1");
            }
        }
    }
}
